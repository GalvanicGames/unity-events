using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public class TestJobFilter : MonoBehaviour
{
	public bool runTest;

	public List<TestData> data;

	[Serializable]
	public struct TestData
	{
		public int num;
		public byte filter;
	
		public TestData(int num) : this()
		{
			this.num = num;
		}
	
		public TestData(int num, byte filter)
		{
			this.num = num;
			this.filter = filter;
		}
	}

	private void Update()
	{
		if (runTest)
		{
			runTest = false;
			RunTest();
		}
	}

	private void RunTest()
	{
		NativeArray<TestData> nData = new NativeArray<TestData>(2, Allocator.TempJob);
			
		// Won't filter to use...or won't be filtered away? Not sure how to interpret
		nData[0] = new TestData(1, 0);
			
		// Opposite, I expect one of these to end up having a value of 2
		nData[1] = new TestData(1, 1); 
			
		NativeList<int> indexes = new NativeList<int>(2, Allocator.TempJob);

		FilterJob filterJob = new FilterJob();
		filterJob.nData = nData;

		JobHandle handle = filterJob.ScheduleAppend(indexes, 2, 32);
		
		handle.Complete();
		
//		handle = filterJob.ScheduleFilter(indexes, 32);
//		
//		handle.Complete();

		NativeArray<int> filteredData = new NativeArray<int>(indexes.Length, Allocator.TempJob);
			
		GetNewDataJob newDataJob = new GetNewDataJob();
		newDataJob.indexes = indexes;
		newDataJob.nData = nData;
		newDataJob.filteredData = filteredData;


		handle = newDataJob.Schedule(indexes.Length, 32, handle);

		DoubleJob doubleJob = new DoubleJob();
		doubleJob.filteredData = filteredData;

		handle = doubleJob.Schedule(filteredData.Length, 32, handle);

		CopyBackData copyBackJob = new CopyBackData();
		copyBackJob.indexes = indexes;
		copyBackJob.nData = nData;
		copyBackJob.filteredData = filteredData;

		handle = copyBackJob.Schedule(filteredData.Length, 32, handle);

		handle.Complete();

		Debug.Log(nData[0].num + " " + nData[1].num);

		handle.Complete();
		nData.Dispose();
		indexes.Dispose();
		filteredData.Dispose();
	}

	private void RunFilteredIncrementTest()
	{
		NativeArray<TestData> nData = new NativeArray<TestData>(2, Allocator.TempJob);
			
		// Won't filter to use...or won't be filtered away? Not sure how to interpret
		nData[0] = new TestData(0, 0);
			
		// Opposite, I expect one of these to end up having a value of 1
		nData[1] = new TestData(0, 1); 
			
		NativeList<int> indexes = new NativeList<int>(2, Allocator.TempJob);
	
		FilterJob filterJob = new FilterJob();
		filterJob.nData = nData;
	
		JobHandle handle = filterJob.ScheduleFilter(indexes, 32);
		
		// Even forcing a complete here doesn't seem to matter. As best as I can tell the filter doesn't run
		// handle.Complete();
		
		IncrementIndices incJob = new IncrementIndices();
		incJob.indexes = indexes;
		incJob.nData = nData;
		handle = incJob.Schedule(handle);
		
		handle.Complete();
		
		Debug.Log(nData[0].num + " " + nData[1].num);
		
		nData.Dispose();
		indexes.Dispose();
	}
	
	private struct FilterJob : IJobParallelForFilter
	{
		[ReadOnly]
		public NativeArray<TestData> nData;
	
		public bool Execute(int index)
		{
			return nData[index].filter > 0;
		}
	}
	
	private struct IncrementIndices : IJob
	{
		public NativeArray<TestData> nData;
	
		[ReadOnly]
		public NativeList<int> indexes;
	
		public void Execute()
		{
			for (int i = 0; i < indexes.Length; i++)
			{
				TestData value = nData[indexes[i]];
				value.num++;
				nData[indexes[i]] = value;
			}
		}
	}

	[BurstCompile]
	private struct GetNewDataJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<TestData> nData;

		[ReadOnly]
		public NativeList<int> indexes;

		public NativeArray<int> filteredData;

		public void Execute(int index)
		{
			filteredData[index] = nData[indexes[index]].num;
		}
	}

	[BurstCompile]
	private struct DoubleJob : IJobParallelFor
	{
		public NativeArray<int> filteredData;

		public void Execute(int index)
		{
			int num = filteredData[index];
			num *= 2;
			filteredData[index] = num;
		}
	}

	[BurstCompile]
	private struct CopyBackData : IJobParallelFor
	{
		[WriteOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<TestData> nData;

		[ReadOnly]
		public NativeList<int> indexes;

		[ReadOnly]
		public NativeArray<int> filteredData;

		public void Execute(int index)
		{
			nData[indexes[index]] = new TestData(filteredData[index]);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class TestBurst : MonoBehaviour
{
	public int numAmount;
	public Text text;
	public Text text2;

	private void Update()
	{
		LongJob job = new LongJob();
		job.numAmount = numAmount;
		job.v1 = 1;
		job.v2 = 2;
		job.v3 = 3;
		job.result = 1;

		float time = Time.realtimeSinceStartup;
		job.Schedule().Complete();
		text.text = ((Time.realtimeSinceStartup - time) * 1000).ToString("F2");
		
		BurstLongJob burstJob = new BurstLongJob();
		burstJob.numAmount = numAmount;
		burstJob.v1 = 1;
		burstJob.v2 = 2;
		burstJob.v3 = 3;
		burstJob.result = 1;

		time = Time.realtimeSinceStartup;
		burstJob.Schedule().Complete();
		text2.text = ((Time.realtimeSinceStartup - time) * 1000).ToString("F2");
	}

	private struct LongJob : IJob
	{
		public int numAmount;
		public float v1;
		public float v2;
		public float v3;

		public float result;

		public void Execute()
		{
			result = 1;

			for (int i = 0; i < numAmount; i++)
			{
				result += 3 * v1 + v2 * v3 + i;
			}
		}
	}
	
	[BurstCompile]
	private struct BurstLongJob : IJob
	{
		public int numAmount;
		public float v1;
		public float v2;
		public float v3;

		public float result;

		public void Execute()
		{
			result = 1;
			
			for (int i = 0; i < numAmount; i++)
			{
				result += 3 * v1 + v2 * v3 + i;
			}
		}
	}
}
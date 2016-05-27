using UnityEngine;
using UnityEvents;

public class TestAttributes : MonoBehaviour
{
	private struct MyTestEvent
	{
		
	}

	
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		gameObject.SetActive(false);
		gameObject.SetActive(true);
	}

	[LocalEventListener]
	private void LocalEventFunction(MyTestEvent ev)
	{
		//Debug.Log("local event!");
	}

	[GlobalEventListener]
	private void GlobalEventFunction(MyTestEvent ev)
	{
		//Debug.Log("global event!");
	}
}

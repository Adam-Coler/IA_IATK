using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    public string String = "hello";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MyFunction(String);
    }

    void MyFunction(string String)
    {
        Debug.Log(String);
    }
}


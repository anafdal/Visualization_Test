﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class DataPlotter5D : MonoBehaviour
{

    // Name of the input file, no extension
    public string inputfile1;
    public string inputfile2;
    public string inputfile3;

    public string dataset;

    // List for holding data from CSV reader
    ArrayList database = new ArrayList();
    //private List<Dictionary<string, object>> dataList1;
    //private List<Dictionary<string, object>> dataList2;
    //private List<Dictionary<string, object>> dataList3;

    //list
    private List<float> NO2 = new List<float>();//list of all the total cases
    private List<float> SO2 = new List<float>();//list of all the total cases
    private List<float> PM10 = new List<float>();


    //column names
    private string geoArea;
    private string no2Rate;
    private string so2Rate;
    private string pm10Rate;


    //scales
    public float plotScale;
    public float sizeScale;
    public float yScale;
    public float zScale;
    public float xScale;

    // The prefab for the data points that will be instantiated
    public GameObject PointPrefab;

    //other
    private List<string> columnList1;
    private List<string> columnList2;
    private List<string> columnList3;


    //y-labels
    public TMP_Text y_min;
    public TMP_Text y_mid;
    public TMP_Text y_max;


    // Object which will contain instantiated prefabs in hiearchy
    public GameObject PointHolder;

    // Use this for initialization
    void Start()
    {
        List<Dictionary<string, object>> dataList1 = CSVReader.Read(inputfile1);
        List<Dictionary<string, object>> dataList2 = CSVReader.Read(inputfile2);
        List<Dictionary<string, object>> dataList3 = CSVReader.Read(inputfile3);
        //database.Add(dataList1);
        //database.Add(dataList2);
        //database.Add(dataList3);

        PlotPoints(dataList1,dataList2,dataList3);
    }

    public void PlotPoints(List<Dictionary<string, object>> dataList1, List<Dictionary<string, object>> dataList2, List<Dictionary<string, object>> dataList3)
    {
        // Declare list of strings, fill with keys (column names)
        columnList1 = new List<string>(dataList1[1].Keys);
        columnList2 = new List<string>(dataList2[1].Keys);
        columnList3 = new List<string>(dataList3[1].Keys);

        geoArea = columnList1[0];//column for states

        no2Rate = columnList1[1];//column for NO2
        so2Rate = columnList2[1];//column for SO2
        pm10Rate = columnList3[1];//column for PM10 consumption


        for (var j = 1; j < columnList1.Count; j++)//through columns for dates
        {
            float z = j;//per date
            no2Rate = columnList1[j];//column for date
            so2Rate = columnList2[j];
            pm10Rate = columnList3[j];


            NO2 = ChangeDate(NO2, no2Rate, dataList1);
            SO2 = ChangeDate(SO2, so2Rate, dataList2);
            PM10 = ChangeDate(PM10, pm10Rate, dataList3);

            float zdef = zScale * z;

            GetYLabel(dataList3);//assign y labels

            //Loop through Pointlist
            for (var i = 0; i < dataList1.Count; i++)//go through row for states
            {
                float x = i;//per state

                
                float normalPM10 = Statistics.normalizeValue(getMin(pm10Rate, dataList3, columnList3), getMax(pm10Rate, dataList3, columnList3), PM10[i]);//make a list so you can normalize the whole thing


                float y = normalPM10;
                float ydef = yScale * y;//use third axis as well
                float xdef = x * xScale;

                //float ydef = (float)0.01 * y;



                // Instantiate as gameobject variable so that it can be manipulated within loop
                GameObject dataPoint = Instantiate(
                        PointPrefab,
                        new Vector3(xdef, ydef, zdef) * plotScale,
                        Quaternion.identity);

                ///Color

                if (dataset != "3D")
                {
                   
                    float normalSO2 = Statistics.normalizeValue(getMin(so2Rate, dataList2, columnList2), getMax(so2Rate, dataList2, columnList2), SO2[i]);//make a list so you can normalize the whole thing


                    Color blueColor = new Color();
                    ColorUtility.TryParseHtmlString("#2166AC", out blueColor);
                    Color redColor = new Color();
                    ColorUtility.TryParseHtmlString("#B2182B", out redColor);
                    Color whiteColor = new Color();
                    ColorUtility.TryParseHtmlString("#F7F7F7", out whiteColor);



                    dataPoint.GetComponent<Renderer>().material.color = Slerp3(blueColor, whiteColor, redColor, normalSO2);//HSB:(https://colorbrewer2.org/#type=diverging&scheme=RdBu&n=3)
                                                                                                                           //dataPoint.transform.localScale = new Vector3(sizeScale, sizeScale, sizeScale);

                    if (dataset == "5D")
                    {
                        float normalNO2 = Statistics.normalizeValue(getMin(no2Rate, dataList1, columnList1), getMax(no2Rate, dataList1, columnList1), NO2[i]);//make a list so you can normalize the whole thing
                        //dataPoint.GetComponent<Renderer>().material.color = Lerp3(Color.blue, Color.white, Color.red, Mathf.PingPong(normalNO2, 1));

                        dataPoint.transform.localScale = new Vector3(normalNO2 * sizeScale, normalNO2 * sizeScale, normalNO2 * sizeScale);//size interpolation by SO2
                    }

                }

                //new Vector3(normalVal*100, y, z) * plotScale

                // Make child of PointHolder object, to keep points within container in hiearchy
                dataPoint.transform.parent = PointHolder.transform;

                // Assigns original values to dataPointName
                string dataPointName =
                    "City: " + dataList1[i][geoArea] + "\n" + //state
                    " Month: " + columnList1[j];   //date

                string dataNeeded = " NO2 Emission: " + dataList1[i][no2Rate] + "\n " +//NO2 cases
                    " SO2 Emission: " + SO2[i] + "\n" +        //SO2 rate
                    " PM10 Fuel Consumption: " + PM10[i];  //PM10 rate
                                                           //+ " Nomral NO2" + normalNO2;*/

                // Assigns name to the prefab
                dataPoint.transform.name = dataPointName + "\n" + dataNeeded;



                // Gets material color and sets it to a new RGB color we define

            }
        }
    }
    public float getMin(string rate, List<Dictionary<string, object>> dataList, List<string> columnList)
    {
        //min1 = Statistics.FindMinValue3(no2Rate, dataList1, columnList1);
        //min3 = Statistics.FindMinValue3(so2Rate, dataList2, columnList2);
        //min5 = Statistics.FindMinValue3(pm10Rate, dataList3, columnList3);
        float min = Statistics.FindMinValue3(rate, dataList, columnList);
        return min;
    }

    public float getMax(string rate, List<Dictionary<string, object>> dataList, List<string> columnList)
    {
        //max2 = Statistics.FindMaxValue3(no2Rate, dataList1, columnList1);
        //max4 = Statistics.FindMaxValue3(so2Rate, dataList2, columnList2);
        //max6 = Statistics.FindMaxValue3(pm10Rate, dataList3, columnList3);

        float max = Statistics.FindMaxValue3(rate, dataList, columnList);
        return max;
    }
    private void GetYLabel(List<Dictionary<string, object>> dataList3)
    {
        // Set y Labels by finding game objects and setting TextMesh and assigning value (need to convert to string)
        y_min.text = getMin(pm10Rate, dataList3, columnList3).ToString("0.0");
        y_mid.text = (getMin(pm10Rate, dataList3, columnList3) + (getMax(pm10Rate, dataList3, columnList3) - getMin(pm10Rate, dataList3, columnList3)) / 2f).ToString("0.0");
        y_max.text = getMax(pm10Rate, dataList3, columnList3).ToString("0.0");

        //set position
        y_min.transform.position = new Vector3(y_min.transform.position.x, Statistics.normalizeValue(getMin(pm10Rate, dataList3, columnList3), getMax(pm10Rate, dataList3, columnList3), getMin(pm10Rate, dataList3, columnList3)) * yScale * plotScale, y_min.transform.position.z);
        y_max.transform.position = new Vector3(y_max.transform.position.x, Statistics.normalizeValue(getMin(pm10Rate, dataList3, columnList3), getMax(pm10Rate, dataList3, columnList3), getMax(pm10Rate, dataList3, columnList3)) * yScale * plotScale, y_max.transform.position.z);

        y_mid.transform.position = new Vector3(y_mid.transform.position.x, (y_min.transform.position.y + (y_max.transform.position.y - y_min.transform.position.y) / 2f), y_mid.transform.position.z);

    }


    static List<float> ChangeDate(List<float> Case, string valueRate, List<Dictionary<string, object>> dataList)
    {
        float[] tempValue = new float[dataList.Count];//temporary array, a placeholder for the values
        Case.Clear();

        for (var n = 0; n < dataList.Count; n++)
        {

            tempValue[n] = System.Convert.ToSingle(dataList[n][valueRate]);//add previous values
            Case.Add(tempValue[n]);

        }

        //Debug.Log(temporary.Count);
        return Case;
    }

    Color Slerp3(Color a, Color b, Color c, float t)
    {
        if (t < 0.5f) // 0.0 to 0.5 goes to a -> b
            return (LABColor.Lerp(LABColor.FromColor(a), LABColor.FromColor(b), t / 0.5f)).ToColor();
        else // 0.5 to 1.0 goes to b -> c
            return (LABColor.Lerp(LABColor.FromColor(b), LABColor.FromColor(c), (t - 0.5f) / 0.5f)).ToColor();
    }

}


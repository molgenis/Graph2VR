using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LimitSlider : MonoBehaviour
{
  public Slider limitSlider;
  public TextMeshProUGUI sliderDisplayValue;
  public TextMeshProUGUI startValue;
  public TextMeshProUGUI endValue;
  public List<int> sliderMarks = new List<int>();
  public RectTransform slider;
  public GameObject markPrefab;

  // Start is called before the first frame update
  void Start()
  {
    startValue.text = sliderMarks[0].ToString();
    endValue.text = sliderMarks[sliderMarks.Count - 1].ToString();

    limitSlider.wholeNumbers = true;
    limitSlider.minValue = 0;
    limitSlider.maxValue = sliderMarks.Count - 1;
    limitSlider.value = 6;

    float markXStep = (360) / (sliderMarks.Count - 1);
    for (int i = 0; i < sliderMarks.Count; i++)
    {
      GameObject clone = Instantiate(markPrefab, slider);
      RectTransform transform = clone.GetComponent<RectTransform>();

      transform.localPosition = new Vector3(-180 + (i * markXStep), 0, 0);
      //transform.offsetMin = new Vector2(-190 + (i * markXStep), 0);
      transform.sizeDelta = new Vector2(10, 20);
    }
  }

  // Update is called once per frame
  void Update()
  {
    int selectedValue = sliderMarks[(int)limitSlider.value];
    sliderDisplayValue.text = selectedValue.ToString();
    QueryService.Instance.queryLimit = selectedValue;
  }
}

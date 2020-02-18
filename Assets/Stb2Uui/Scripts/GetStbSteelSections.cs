using System.Xml.Linq;

using UnityEngine;

public partial class STBReader : MonoBehaviour {

    static void GetStbSteelSection(XDocument xDoc, string xDateTag, string sectionType) {
        if (sectionType == "Pipe") {
            var xSteelSections = xDoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                m_xStName.Add((string)xSteelSection.Attribute("name"));
                m_xStParamA.Add((float)xSteelSection.Attribute("D"));
                m_xStParamB.Add((float)xSteelSection.Attribute("t"));
                m_xStType.Add(sectionType);
            }
        }
        else if (sectionType == "Bar") {
            var xSteelSections = xDoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                m_xStName.Add((string)xSteelSection.Attribute("name"));
                m_xStParamA.Add((float)xSteelSection.Attribute("R"));
                m_xStParamB.Add(0);
                m_xStType.Add(sectionType);
            }
        }
        else if (sectionType == "NotSupport") {
        }
        else {
            var xSteelSections = xDoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                m_xStName.Add((string)xSteelSection.Attribute("name"));
                m_xStParamA.Add((float)xSteelSection.Attribute("A"));
                m_xStParamB.Add((float)xSteelSection.Attribute("B"));
                m_xStType.Add(sectionType);
            }
        }
    }
}

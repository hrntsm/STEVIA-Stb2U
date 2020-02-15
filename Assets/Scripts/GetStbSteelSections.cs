using System.Xml.Linq;

using UnityEngine;

public partial class STBReader : MonoBehaviour {

    void GetStbSteelSection(XDocument xDoc, string xDateTag, string sectionType) {
        if (sectionType == "Pipe") {
            var xSteelSections = xDoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                m_xSteelName.Add((string)xSteelSection.Attribute("name"));
                m_xSteelParamA.Add((float)xSteelSection.Attribute("D"));
                m_xSteelParamB.Add((float)xSteelSection.Attribute("t"));
                m_xSteelType.Add(sectionType);
            }
        }
        else if (sectionType == "Bar") {
            var xSteelSections = xDoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                m_xSteelName.Add((string)xSteelSection.Attribute("name"));
                m_xSteelParamA.Add((float)xSteelSection.Attribute("R"));
                m_xSteelParamB.Add(0);
                m_xSteelType.Add(sectionType);
            }
        }
        else if (sectionType == "NotSupport") {
        }
        else {
            var xSteelSections = xDoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                m_xSteelName.Add((string)xSteelSection.Attribute("name"));
                m_xSteelParamA.Add((float)xSteelSection.Attribute("A"));
                m_xSteelParamB.Add((float)xSteelSection.Attribute("B"));
                m_xSteelType.Add(sectionType);
            }
        }
    }
}

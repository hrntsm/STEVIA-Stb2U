using System.Xml.Linq;

using UnityEngine;

public partial class STBReader : MonoBehaviour {

    static void GetStbSteelSection(XDocument xdoc, string xDateTag, string SectionType) {
        if (SectionType == "Pipe") {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((float)xSteelSection.Attribute("D"));
                xStbSecSteelParamB.Add((float)xSteelSection.Attribute("t"));
                xStbSecSteelType.Add(SectionType);
            }
        }
        else if (SectionType == "Bar") {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((float)xSteelSection.Attribute("R"));
                xStbSecSteelParamB.Add(0);
                xStbSecSteelType.Add(SectionType);
            }
        }
        else if (SectionType == "NotSupport") {
        }
        else {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((float)xSteelSection.Attribute("A"));
                xStbSecSteelParamB.Add((float)xSteelSection.Attribute("B"));
                xStbSecSteelType.Add(SectionType);
            }
        }
    }
}

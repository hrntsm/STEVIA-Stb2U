using System.Xml.Linq;
using UnityEngine;

namespace Stevia {

    public partial class STBReader:MonoBehaviour {

        void GetStbSteelSection(XDocument xDoc, string xDateTag, string sectionType) {
            if (sectionType == "Pipe") {
                var xSteelSections = xDoc.Root.Descendants(xDateTag);
                foreach (var xSteelSection in xSteelSections) {
                    _xStName.Add((string)xSteelSection.Attribute("name"));
                    _xStParamA.Add((float)xSteelSection.Attribute("t"));
                    _xStParamB.Add((float)xSteelSection.Attribute("D"));
                    _xStType.Add(sectionType);
                }
            }
            else if (sectionType == "Bar") {
                var xSteelSections = xDoc.Root.Descendants(xDateTag);
                foreach (var xSteelSection in xSteelSections) {
                    _xStName.Add((string)xSteelSection.Attribute("name"));
                    _xStParamA.Add((float)xSteelSection.Attribute("R"));
                    _xStParamB.Add(0);
                    _xStType.Add(sectionType);
                }
            }
            else if (sectionType == "NotSupport") {
            }
            else {
                var xSteelSections = xDoc.Root.Descendants(xDateTag);
                foreach (var xSteelSection in xSteelSections) {
                    _xStName.Add((string)xSteelSection.Attribute("name"));
                    _xStParamA.Add((float)xSteelSection.Attribute("A"));
                    _xStParamB.Add((float)xSteelSection.Attribute("B"));
                    _xStType.Add(sectionType);
                }
            }
        }
    }
}

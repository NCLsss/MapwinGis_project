﻿using MWLite.Symbology.Forms.Symbology;
using MapWinGIS;
using System.Windows.Forms;
using MWLite.Symbology.Classes;

namespace MWLite.Symbology.Classes
{
    public static class FormHelper
    {
        /// <summary>
        /// Displays symbology form of the appropriate type
        /// </summary>
        public static Form GetSymbologyForm(LegendControl.Legend legend, int layerHandle, ShpfileType type, ShapeDrawingOptions options, bool applyDisabled)
        {
            Form form = null;
            var shpType = Globals.ShapefileType2D(type);
            var layer = legend.Layers.ItemByHandle(layerHandle);

            if (shpType == ShpfileType.SHP_POINT || shpType == ShpfileType.SHP_MULTIPOINT)
            {
                form = new PointsForm(legend, layer, options, applyDisabled);
            }
            else if (shpType == ShpfileType.SHP_POLYLINE)
            {
                form = new LinesForm(legend, layer, options, applyDisabled);
            }
            else if (shpType == ShpfileType.SHP_POLYGON)
            {
                form = new PolygonsForm(legend, layer, options, applyDisabled);
            }
            return form;
        }
    }
}

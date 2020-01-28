using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CenturyLink.Network.Engineering.Common.Configuration;
using CenturyLink.Network.Engineering.Common.Utility;
using NLog;

namespace CenturyLink.Network.Engineering.Material.Editor.Models.Template
{
    public class Scale
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string CANVAS_WIDTH = "canvasWidthInPixels";
        private const string CANVAS_HEIGHT = "canvasHeightInPixels";
        private const string PERCENT_TO_USE = "percentOfCanvasToUse";
        private int canvasWidth = 0;
        private int canvasHeight = 0;
        private int percentOfCanvasToUse = 0;
        private int top = 0;
        private int left = 0;
        private decimal pixelsPerInch = 0;

        private Scale()
        {

        }

        public Scale(decimal objectHeight, decimal objectWidth, string unitOfMeasure)
        {
            if (!int.TryParse(ConfigurationManager.Value(APPLICATION_NAME.CDMMS_DRAWING, CANVAS_HEIGHT), out canvasHeight))
                throw new Exception(String.Format("SYSTEM_CONFIGURATION table missing value {0}.{1}", Constants.ApplicationName(APPLICATION_NAME.CDMMS_DRAWING), CANVAS_HEIGHT));

            if (!int.TryParse(ConfigurationManager.Value(APPLICATION_NAME.CDMMS_DRAWING, CANVAS_WIDTH), out canvasWidth))
                throw new Exception(String.Format("SYSTEM_CONFIGURATION table missing value {0}.{1}", Constants.ApplicationName(APPLICATION_NAME.CDMMS_DRAWING), CANVAS_WIDTH));

            if (!int.TryParse(ConfigurationManager.Value(APPLICATION_NAME.CDMMS_DRAWING, PERCENT_TO_USE), out percentOfCanvasToUse))
                throw new Exception(String.Format("SYSTEM_CONFIGURATION table missing value {0}.{1}", Constants.ApplicationName(APPLICATION_NAME.CDMMS_DRAWING), PERCENT_TO_USE));

            CalculateScale(objectHeight, objectWidth, unitOfMeasure);
        }

        public decimal ConvertPhysicalDimensionToPixels(decimal dimension)
        {
            return pixelsPerInch * dimension;
        }

        public decimal ConvertPixelsToPhysicalDimension(decimal pixels)
        {
            throw new NotImplementedException();
        }

        public int Top
        {
            get
            {
                return top;
            }
            set
            {
                top = value;
            }
        }

        public int Left
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
            }
        }

        private void CalculateScale(decimal height, decimal width, string unitOfMeasure)
        {
            //TODO: assuming unitOfMeasure is in inches, check for other units and do conversion
            decimal largerDimension = 0;
            decimal usableCanvasDimension = 0;
            decimal canvasBorder = 0;
            decimal canvasBorderAsPercent = decimal.Divide((100M - percentOfCanvasToUse), 100M);

            if (height > width)
            {
                usableCanvasDimension = decimal.Multiply(canvasHeight, decimal.Divide(percentOfCanvasToUse, 100M));
                canvasBorder = decimal.Divide(decimal.Multiply(canvasHeight, canvasBorderAsPercent), 2);
                largerDimension = height;
            }
            else
            {
                usableCanvasDimension = decimal.Multiply(canvasWidth, decimal.Divide(percentOfCanvasToUse, 100M));
                canvasBorder = decimal.Divide(decimal.Multiply(canvasWidth, canvasBorderAsPercent), 2);
                largerDimension = width;
            }

            top = decimal.ToInt32(canvasBorder);
            left = decimal.ToInt32(canvasBorder);
            pixelsPerInch = decimal.Divide(usableCanvasDimension, largerDimension);
        }
    }
}
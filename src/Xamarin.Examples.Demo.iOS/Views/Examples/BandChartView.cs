﻿using SciChart.Examples.Demo.Data;
using SciChart.Examples.Demo.Fragments.Base;
using SciChart.iOS.Charting;
using UIKit;
using Xamarin.Examples.Demo.iOS.Resources.Layout;
using Xamarin.Examples.Demo.iOS.Views.Base;

namespace Xamarin.Examples.Demo.iOS.Views.Examples
{
    [ExampleDefinition("Band Chart", "Generates a simple Band series chart in code", icon: ExampleIcon.BandChart)]
    public class BandChartView : ExampleBaseView<SingleChartViewLayout>
    {
        private readonly SingleChartViewLayout _exampleViewLayout = SingleChartViewLayout.Create();
        public override SingleChartViewLayout ExampleViewLayout => _exampleViewLayout;

        public SCIChartSurface Surface => ExampleViewLayout.SciChartSurface;
    
        protected override void UpdateFrame()
        {
			Surface.TranslatesAutoresizingMaskIntoConstraints = false;

			NSLayoutConstraint constraintRight = NSLayoutConstraint.Create(Surface, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1, 0);
			NSLayoutConstraint constraintLeft = NSLayoutConstraint.Create(Surface, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0);
			NSLayoutConstraint constraintTop = NSLayoutConstraint.Create(Surface, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0);
			NSLayoutConstraint constraintBottom = NSLayoutConstraint.Create(Surface, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, 0);

			this.AddConstraint(constraintRight);
			this.AddConstraint(constraintLeft);
			this.AddConstraint(constraintTop);
			this.AddConstraint(constraintBottom);
        }

        protected override void InitExampleInternal()
        {
            var data0 = DataManager.Instance.GetDampedSinewave(1.0, 0.01, 1000);
            var data1 = DataManager.Instance.GetDampedSinewave(1.0, 0.005, 1000, 12);

            var dataSeries = new XyyDataSeries<double, double>();
            dataSeries.Append(data0.XData, data0.YData, data1.YData);

            var xAxis = new SCINumericAxis {VisibleRange = new SCIDoubleRange(1.1, 2.7)};
            var yAxis = new SCINumericAxis {GrowBy = new SCIDoubleRange(0.1, 0.1)};

            var renderSeries = new SCIFastBandRenderableSeries
            {
                DataSeries = dataSeries,
                StrokeStyle = new SCISolidPenStyle(0xFFFF1919, 0.7f),
                StrokeY1Style = new SCISolidPenStyle(0xFF279B27, 0.7f),
                FillBrushStyle = new SCISolidBrushStyle(0x33279B27),
                FillY1BrushStyle = new SCISolidBrushStyle(0x33FF1919)
            };

            Surface.XAxes.Add(xAxis);
            Surface.YAxes.Add(yAxis);
            Surface.RenderableSeries.Add(renderSeries);

            Surface.ChartModifiers = new SCIChartModifierCollection(
                new SCIZoomPanModifier(),
                new SCIPinchZoomModifier(),
                new SCIZoomExtentsModifier()
            );

            Surface.InvalidateElement();
        }
    }
}
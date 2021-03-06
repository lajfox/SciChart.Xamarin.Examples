﻿using System;
using System.Linq;
using SciChart.Examples.Demo.Data;
using SciChart.Examples.Demo.Fragments.Base;
using SciChart.iOS.Charting;
using UIKit;
using Xamarin.Examples.Demo.Data;
using Xamarin.Examples.Demo.iOS.Resources.Layout;
using Xamarin.Examples.Demo.iOS.Views.Base;

namespace Xamarin.Examples.Demo.iOS.Views.Examples
{
    [ExampleDefinition("Realtime Ticking Stock Charts", description: "Creates a realtime stock chart which ticks and updates, simulating live a market", icon: ExampleIcon.RealTime)]
    public class CreateRealtimeTickingStockChartsView : ExampleBaseView<RealtimeTickingStockChartsLayout>
    {
        private const int DefaultPointCount = 150;
        private const uint SmaSeriesColor = 0xFFFFA500;
        private const uint StrokeUpColor = 0xFF00AA00;
        private const uint StrokeDownColor = 0xFFFF0000;
        private const float StrokeThickness = 1.5f;

        private readonly OhlcDataSeries<DateTime, double> _ohlcDataSeries = new OhlcDataSeries<DateTime, double>(SCITypeOfDataSeries.XCategory) { SeriesName = "Price Series" };
        private readonly XyDataSeries<DateTime, double> _xyDataSeries = new XyDataSeries<DateTime, double>(SCITypeOfDataSeries.XCategory) { SeriesName = "50-Period SMA" };

        private SCIAxisMarkerAnnotation _smaAxisMarker;
        private SCIAxisMarkerAnnotation _ohlcAxisMarker;

        // Create data service to populate the data
        private readonly IMarketDataService _marketDataService = new MarketDataService(new DateTime(2000, 08, 01, 12, 00, 00), 5, 20);
        private readonly MovingAverage _sma50 = new MovingAverage(50);
        private PriceBar _lastPrice;

        private readonly RealtimeTickingStockChartsLayout _exampleViewLayout = RealtimeTickingStockChartsLayout.Create();
        public override RealtimeTickingStockChartsLayout ExampleViewLayout => _exampleViewLayout;

        public SCIChartSurface MainSurface => ExampleViewLayout.MainSurfaceView;
        public SCIChartSurface OverviewSurface => ExampleViewLayout.OverviewSurfaceView;

        protected override void UpdateFrame()
        {
        }

        protected override void InitExampleInternal()
        {
            var prices = _marketDataService.GetHistoricalData(DefaultPointCount);

            // Populate data series with some data
            _ohlcDataSeries.Append(prices.Select(x => x.DateTime),
                prices.Select(x => x.Open),
                prices.Select(x => x.High),
                prices.Select(x => x.Low),
                prices.Select(x => x.Close));
            _xyDataSeries.Append(prices.Select(x => x.DateTime), prices.Select(y => _sma50.Push(y.Close).Current));

            CreateMainPriceChart();
            // CREATE THE SECONDARY OVERVIEW CHART 
            SCIBoxAnnotation overviewAnnotation0;
            SCIBoxAnnotation overviewAnnotation1;
            CreateOverviewChart(out overviewAnnotation0, out overviewAnnotation1);

            // Link the main price chart to the second
            // On zoom and pan of the main price chart, we want to update
            // the position of annotations on the second chart so that you can 
            // see the area of the data zoomed in (called an 'overview')

            //((SCIAxisBase)MainSurface.XAxes[0]).VisibleRangeChange += (s, e) =>

            var axis0 = MainSurface.XAxes[0];
            var axis = (SCIAxisBase)axis0;

            var callback = new SCIAxisVisibleRangeChanged((ISCIRangeProtocol arg0, ISCIRangeProtocol arg1, bool arg2, Foundation.NSObject arg3) =>
            {
                // Left annotation starts on the left edge of the chart and ends on the right edge 
                // of the visible area 
                var overviewXAxis = OverviewSurface.XAxes.ItemAt(0);
                var mainXAxis = MainSurface.XAxes.ItemAt(0);
                var overviewVisibleRange = overviewXAxis.VisibleRange;
                var dOverviewVisibleRange = overviewVisibleRange.AsDoubleRange();

                double min0X1 = dOverviewVisibleRange.Min;
                double min0X2 = mainXAxis.VisibleRange.AsDoubleRange().Min;
                overviewAnnotation0.X1Value = min0X1;
                overviewAnnotation0.X2Value = min0X2;

                // Right annotation starts on the right edge of visible area and ends on the right
                // edge of all the data 
                double min1X1 = mainXAxis.VisibleRange.AsDoubleRange().Max;
                double min1X2 = overviewXAxis.VisibleRange.AsDoubleRange().Max;
                overviewAnnotation1.X1Value = min1X1;
                overviewAnnotation1.X2Value = min1X2;
            });

            axis.RegisterVisibleRangeChangedCallback(callback);
        }

        private void CreateMainPriceChart()
        {
            // Create an XAxis and YAxis for our chart
            var xAxis = new SCICategoryDateTimeAxis
            {
                Style = { DrawMajorGridLines = false },
                GrowBy = new SCIDoubleRange(0, 0.1)
            };
            var yAxis = new SCINumericAxis { AutoRange = SCIAutoRange.Always };

            // Create RenderableSeries to render the data 
            var ohlcSeries = new SCIFastOhlcRenderableSeries
            {
                DataSeries = _ohlcDataSeries,
                StrokeUpStyle = new SCISolidPenStyle(StrokeUpColor, StrokeThickness),
                StrokeDownStyle = new SCISolidPenStyle(StrokeDownColor, StrokeThickness),
            };

            var movingAverage50Series = new SCIFastLineRenderableSeries { DataSeries = _xyDataSeries, StrokeStyle = new SCISolidPenStyle(0xFFFF6600, 1.0f) };

            // Create axis markers annotations to show the last values on real-time chart
            //TODO Position -> Y1Value, color property should be on annotation itself
            _smaAxisMarker = new SCIAxisMarkerAnnotation { Position = 0d };
            _smaAxisMarker.Style.BackgroundColor = SmaSeriesColor.ToColor();

            _ohlcAxisMarker = new SCIAxisMarkerAnnotation { Position = 0d };
            _ohlcAxisMarker.Style.BackgroundColor = StrokeUpColor.ToColor();

            MainSurface.XAxes.Add(xAxis);
            MainSurface.YAxes.Add(yAxis);
            MainSurface.RenderableSeries.Add(ohlcSeries);
            MainSurface.RenderableSeries.Add(movingAverage50Series);
            MainSurface.Annotations.Add(_ohlcAxisMarker);
            MainSurface.Annotations.Add(_smaAxisMarker);

            // Populate some pinch and touch interactions. Pinch to zoom, drag to pan and double-tap to zoom extents 
            MainSurface.ChartModifiers = new SCIChartModifierCollection(
                new SCIXAxisDragModifier(),
                // TODO XyDirection should be Direction; SCIXYDirection should be Direction2D
                new SCIZoomPanModifier { XyDirection = SCIXYDirection.XDirection },
                new SCIZoomExtentsModifier(),
                new SCILegendCollectionModifier { Orientation = SCIOrientation.Horizontal }
            );
        }

        private void CreateOverviewChart(out SCIBoxAnnotation leftAreaAnnotation, out SCIBoxAnnotation rightAreaAnnotation)
        {
            // Create an XAxis and YAxis for our chart
            var xAxis1 = new SCICategoryDateTimeAxis { AutoRange = SCIAutoRange.Always };
            var yAxis1 = new SCINumericAxis { GrowBy = new SCIDoubleRange(0.1, 0.1), AutoRange = SCIAutoRange.Always };

            // Create the mountain chart for the overview , using the same price data but zoomed out 
            var mountainSeries = new SCIFastMountainRenderableSeries { DataSeries = _ohlcDataSeries };

            // Create some annotations to visualize the selected area on the main price chart 
            leftAreaAnnotation = new SCIBoxAnnotation
            {
                CoordinateMode = SCIAnnotationCoordinateMode.RelativeY,
                Y1Value = 0,
                Y2Value = 1,
                Style = new SCIBoxAnnotationStyle
                {
                    FillBrush = new SCISolidBrushStyle(0x33FFFFFF),
                },
            };

            rightAreaAnnotation = new SCIBoxAnnotation
            {
                CoordinateMode = SCIAnnotationCoordinateMode.RelativeY,
                Y1Value = 0,
                Y2Value = 1,
                Style = new SCIBoxAnnotationStyle
                {
                    FillBrush = new SCISolidBrushStyle(0x33FFFFFF),
                },
            };

            // Populate the chart with Axis, RenderableSeries. The chart automatically updates when any property changes 
            OverviewSurface.XAxes.Add(xAxis1);
            OverviewSurface.YAxes.Add(yAxis1);
            OverviewSurface.RenderableSeries.Add(mountainSeries);

            OverviewSurface.Annotations.Add(leftAreaAnnotation);
            OverviewSurface.Annotations.Add(rightAreaAnnotation);
        }

        private void OnNewPrice(PriceBar price)
        {
            // Ensure only one update processed at a time from multi-threaded timer
            lock (this)
            {
                // Update the last price, or append? 
                double smaLastValue;
                if (_lastPrice.DateTime == price.DateTime)
                {
                    _ohlcDataSeries.Update(_ohlcDataSeries.Count - 1, price.Open, price.High, price.Low, price.Close);

                    smaLastValue = _sma50.Update(price.Close).Current;
                    _xyDataSeries.UpdateYAt(_xyDataSeries.Count - 1, smaLastValue);
                }
                else
                {
                    _ohlcDataSeries.Append(price.DateTime, price.Open, price.High, price.Low, price.Close);

                    smaLastValue = _sma50.Push(price.Close).Current;
                    _xyDataSeries.Append(price.DateTime, smaLastValue);

                    // If the latest appending point is inside the viewport (i.e. not off the edge of the screen)
                    // then scroll the viewport 1 bar, to keep the latest bar at the same place
                    // TODO Collections should have indexers
                    var visibleRange = MainSurface.XAxes.ItemAt(0).VisibleRange;
                    if (visibleRange.AsDoubleRange().Max > _ohlcDataSeries.Count)
                    {
                        // TODO Should have set minmax double method on it, to avoid the following code
                        //visibleRange.SetMinMaxDouble(visibleRange.AsDoubleRange().Min + 1, visibleRange.AsDoubleRange().Max + 1);
                        var dateRange = ((SCIDateRange)visibleRange);
                        var tempRange = new SCIDateRange(new DateTime(dateRange.Min.Millisecond + 1), new DateTime(dateRange.Max.Millisecond + 1));
                    }
                }

                _ohlcAxisMarker.Style.BackgroundColor = (price.Close >= price.Open ? StrokeUpColor : StrokeDownColor).ToColor();

                _ohlcAxisMarker.Position = price.Close;
                _smaAxisMarker.Position = smaLastValue;

                _lastPrice = price;
            }
        }
    }
}
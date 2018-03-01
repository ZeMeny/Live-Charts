﻿using System.Windows;
using System.Windows.Controls;
using LiveCharts.Core.Abstractions;
using LiveCharts.Core.Dimensions;
using LiveCharts.Core.Events;
using LiveCharts.Wpf.Animations;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace LiveCharts.Wpf.Views
{
    /// <summary>
    /// The separator class.
    /// </summary>
    /// <seealso cref="ICartesianAxisSeparator" />
    public class CartesianAxisSeparatorView<TLabel> : ICartesianAxisSeparator
        where TLabel : FrameworkElement, IPlaneLabelControl, new()
    {
        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        /// <value>
        /// The line.
        /// </value>
        public Rectangle Rectangle { get; protected set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public TLabel Label { get; protected set; }

        /// <inheritdoc />
        object ICartesianAxisSeparator.VisualElement => Label;

        public void Move(CartesianAxisSeparatorArgs args)
        {
            var isNew = Rectangle == null;
            var isNewLabel = Label == null;
            var speed = args.ChartView.AnimationsSpeed;

            if (isNew)
            {
                Rectangle = new Rectangle();
                Panel.SetZIndex(Rectangle, -1);
                SetInitialLineParams(args);
                args.ChartView.Content.AddChild(Rectangle);
                Rectangle.Animate()
                    .AtSpeed(speed)
                    .Property(UIElement.OpacityProperty, 1, 0)
                    .Begin();
            }

            if (isNewLabel)
            {
                Label = new TLabel();
                SetInitialLabelParams();
                args.ChartView.Content.AddChild(Label);
                Label.Animate()
                    .AtSpeed(speed)
                    .Property(UIElement.OpacityProperty, 1, 0)
                    .Begin();
            }

            var axis = (Axis) args.Plane;
            var style = args.Plane.Dimension == 0
                ? (args.IsAlternative ? axis.XAlternativeSeparatorStyle : axis.XSeparatorStyle)
                : (args.IsAlternative ? axis.YAlternativeSeparatorStyle : axis.YSeparatorStyle);
            var st = double.IsNaN(style.StrokeThickness) ? 0 : style.StrokeThickness;

            Rectangle.Fill = style.Fill.AsWpf();
            Rectangle.Stroke = style.Stroke.AsWpf();

            Label.Measure(args.AxisLabelModel.Content);

            var actualLabelLocation = args.AxisLabelModel.Location + args.AxisLabelModel.Offset;

            var storyboard = Rectangle.Animate()
                .AtSpeed(speed)
                .Property(Canvas.TopProperty, args.Model.Top)
                .Property(Canvas.LeftProperty, args.Model.Left)
                .Property(FrameworkElement.HeightProperty, args.Model.Height > st
                    ? args.Model.Height
                    : st)
                .Property(FrameworkElement.WidthProperty, args.Model.Width > st
                    ? args.Model.Width
                    : st)
                .SetTarget(Label)
                .Property(Canvas.LeftProperty, actualLabelLocation.X)
                .Property(Canvas.TopProperty, actualLabelLocation.Y);

            if (args.Disposing)
            {
                storyboard.Property(UIElement.OpacityProperty, 0)
                    .Then((sender, e) =>
                    {
                        ((IResource) this).Dispose(args.ChartView);
                        storyboard = null;
                    });
            }

            storyboard.Begin();
        }

        private void SetInitialLineParams(CartesianAxisSeparatorArgs args)
        {
            Canvas.SetTop(Rectangle, args.Model.Top);
            Canvas.SetLeft(Rectangle, args.Model.Left);
            Rectangle.Width = args.Model.Width;
            Rectangle.Height = args.Model.Height;
            if (args.Plane.Dimension == 0) // if X
            {

            }
            else
            {

            }
        }

        private void SetInitialLabelParams()
        {
            var label = (UIElement) Label;
            Canvas.SetTop(label, 0);
            Canvas.SetLeft(label, 0);
        }

        public event DisposingResourceHandler Disposed;

        object IResource.UpdateId { get; set; }

        void IResource.Dispose(IChartView chart)
        {
            chart.Content.RemoveChild(Rectangle);
            chart.Content.RemoveChild(Label);
            Rectangle = null;
            Disposed?.Invoke(chart, this);
        }
    }
}
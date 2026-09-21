using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Junevy.Controls.Controls.Button
{
    /// <summary>
    /// 只缩小不放大的等比缩放宿主（jv:Button 模板内部使用，不对外公开）。
    /// 测量时按无约束尺寸取得内容自然尺寸，并将期望尺寸钳制在可用空间内；
    /// 布局时内容按自然尺寸参与排版（不触发重新布局），再经 RenderTransform
    /// 以内容中心为基准视觉等比缩小并保持居中。缩放系数以 1 为上限：
    /// 空间充足时内容保持原始字号，仅当宿主被挤压（最终尺寸小于内容自然尺寸）时缩小。
    /// </summary>
    internal sealed class ShrinkBox : Decorator
    {
        /// <summary>
        /// 内容自然尺寸（缩放系数 1 的基准），在测量阶段取得。
        /// </summary>
        private Size naturalSize;

        /// <summary>
        /// 缓存的等比缩放变换，布局时仅更新系数，避免反复创建变换对象。
        /// </summary>
        private ScaleTransform? scaleTransform;

        /// <summary>
        /// 初始化 ShrinkBox，内容变换以自身渲染尺寸中心为基准。
        /// </summary>
        public ShrinkBox()
        {
            this.RenderTransformOrigin = new Point(0.5d, 0.5d);
        }

        /// <summary>
        /// 测量内容自然尺寸，并将期望尺寸钳制为自然尺寸与可用尺寸的较小值：
        /// 空间充足时按自然尺寸占位（缩放系数为 1），被挤压时收缩占位。
        /// </summary>
        /// <param name="availableSize">父级提供的可用尺寸。</param>
        /// <returns>钳制后的期望尺寸。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.Child != null)
            {
                // 无约束测量内容，取得缩放系数 1 的自然尺寸
                this.Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                this.naturalSize = this.Child.DesiredSize;
            }
            else
            {
                this.naturalSize = new Size(0d, 0d);
            }

            return new Size(
                Math.Min(this.naturalSize.Width, availableSize.Width),
                Math.Min(this.naturalSize.Height, availableSize.Height));
        }

        /// <summary>
        /// 将内容按自然尺寸居中排版，并施加不超过 1 的等比缩放变换：
        /// 最终尺寸不小于自然尺寸时系数为 1（外观与固定字号一致），否则等比缩小。
        /// </summary>
        /// <param name="finalSize">父级分配的最终尺寸。</param>
        /// <returns>使用的最终尺寸。</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElement? child = this.Child;
            if (child == null)
            {
                return finalSize;
            }

            if (this.naturalSize.Width > 0d && this.naturalSize.Height > 0d)
            {
                // 等比系数取两方向的较小值，并以 1 为上限：只缩小，不放大
                double scale = Math.Min(
                    1d,
                    Math.Min(finalSize.Width / this.naturalSize.Width, finalSize.Height / this.naturalSize.Height));

                // 内容按自然尺寸居中参与布局，视觉缩小经 RenderTransform 完成（渲染中心即最终区域中心）
                child.Arrange(new Rect(
                    new Point((finalSize.Width - this.naturalSize.Width) / 2d, (finalSize.Height - this.naturalSize.Height) / 2d),
                    this.naturalSize));
                this.ApplyScale(child, scale);
            }
            else
            {
                child.Arrange(new Rect(finalSize));
            }

            return finalSize;
        }

        /// <summary>
        /// 应用等比缩放系数（复用缓存变换；子元素被替换时重建）。
        /// </summary>
        /// <param name="child">当前子元素。</param>
        /// <param name="scale">缩放系数，不超过 1。</param>
        private void ApplyScale(UIElement child, double scale)
        {
            if (this.scaleTransform == null || !ReferenceEquals(child.RenderTransform, this.scaleTransform))
            {
                this.scaleTransform = new ScaleTransform(1d, 1d);
                child.RenderTransform = this.scaleTransform;
            }

            this.scaleTransform.ScaleX = scale;
            this.scaleTransform.ScaleY = scale;
        }
    }
}

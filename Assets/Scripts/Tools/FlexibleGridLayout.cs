using TMPro;
using UnityEngine;
using UnityEngine.UI;

// for TMP_Text

namespace Tools
{
    [ExecuteAlways]
    public class FlexibleGridLayout : LayoutGroup
    {
        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FixedRows,
            FixedColumns
        }

        [Header("Grid")] public FitType fitType = FitType.Uniform;
        public int rows = 1;
        public int columns = 1;
        public Vector2 cellSize = new Vector2(100, 100); // default fallback for children
        public Vector2 spacing = new Vector2(10, 10);
        public bool fitX;
        public bool fitY;

        [Header("Per-child preferred sizing")] public bool usePerChildPreferred = true; // turn on per-child measuring
        public bool preferredAffectsWidth = false; // usually keep fixed column widths
        public bool preferredAffectsHeight = true; // let text drive height per item
        public bool constrainTMPToColumnWidth = true; // pass col width to TMP for wrapping
        public Vector2 minChildSize = Vector2.zero; // clamp preferred sizes

        // caches
        private Vector2[] _childSizes;
        private float[] _colMax; // max width per column
        private float[] _rowMax; // max height per row

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            int childCount = Mathf.Max(0, rectChildren.Count);

            // Grid shape
            if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
            {
                fitX = true;
                fitY = true;
                float sqr = Mathf.Sqrt(childCount);
                rows = Mathf.CeilToInt(sqr);
                columns = Mathf.CeilToInt(sqr);
            }

            if (fitType == FitType.Width || fitType == FitType.FixedColumns)
            {
                columns = Mathf.Max(1, columns);
                rows = Mathf.CeilToInt(childCount / (float)columns);
            }

            if (fitType == FitType.Height || fitType == FitType.FixedRows)
            {
                rows = Mathf.Max(1, rows);
                columns = Mathf.CeilToInt(childCount / (float)rows);
            }

            rows = Mathf.Max(1, rows);
            columns = Mathf.Max(1, columns);

            float parentW = rectTransform.rect.width;
            float parentH = rectTransform.rect.height;

            float availableW = parentW - padding.left - padding.right - spacing.x * (columns - 1);
            float availableH = parentH - padding.top - padding.bottom - spacing.y * (rows - 1);

            float computedColW = availableW / columns;
            float computedRowH = availableH / rows;

            // Prepare caches
            EnsureCaches(childCount, columns, rows);

            // PASS 1: measure each child
            float widthConstraint = (constrainTMPToColumnWidth && computedColW > 0f) ? computedColW : Mathf.Infinity;

            for (int i = 0; i < childCount; i++)
            {
                var child = rectChildren[i];
                int row = i / columns;
                int col = i % columns;

                // start from fallback
                Vector2 sz = cellSize;

                if (usePerChildPreferred)
                {
                    // try TMP
                    TMP_Text tmp = child.GetComponent<TMP_Text>();
                    if (tmp != null)
                    {
                        tmp.ForceMeshUpdate();
                        // width constraint only if we want wrapping to match column width
                        float wForTMP = preferredAffectsWidth
                            ? widthConstraint
                            : (constrainTMPToColumnWidth ? widthConstraint : Mathf.Infinity);
                        Vector2 pref = tmp.GetPreferredValues(tmp.text, wForTMP, Mathf.Infinity);

                        if (preferredAffectsWidth) sz.x = Mathf.Max(minChildSize.x, pref.x);
                        if (preferredAffectsHeight) sz.y = Mathf.Max(minChildSize.y, pref.y);
                    }
                    else
                    {
                        // generic UI (Image, Button, etc.) via LayoutUtility
                        float w = cellSize.x;
                        float h = cellSize.y;

                        if (preferredAffectsWidth) sz.x = Mathf.Max(minChildSize.x, w);
                        if (preferredAffectsHeight) sz.y = Mathf.Max(minChildSize.y, h);
                    }
                }

                // If you still want fitX/fitY to override, keep these:
                if (fitX) sz.x = computedColW;
                if (fitY) sz.y = computedRowH;

                _childSizes[i] = sz;

                // Track per-column/row max to keep grid alignment
                _colMax[col] = Mathf.Max(_colMax[col], sz.x);
                _rowMax[row] = Mathf.Max(_rowMax[row], sz.y);
            }

            // Compute total size based on per-column/row maxima
            float totalW = padding.left + padding.right;
            for (int c = 0; c < columns; c++)
                totalW += _colMax[c] + (c > 0 ? spacing.x : 0f);
            // fix extra spacing: the loop above adds spacing for every col after first,
            // we want (columns - 1) spacings overall:
            totalW = padding.left + padding.right
                                  + Sum(_colMax) + spacing.x * (columns - 1);

            float totalH = padding.top + padding.bottom
                                       + Sum(_rowMax) + spacing.y * (rows - 1);

            SetLayoutInputForAxis(totalW, totalW, -1, 0);
            SetLayoutInputForAxis(totalH, totalH, -1, 1);

            // PASS 2: position children
            SetCells();
        }

        public override void CalculateLayoutInputVertical()
        {
            /* already handled */
        }

        public override void SetLayoutHorizontal() => SetCells();
        public override void SetLayoutVertical() => SetCells();

        private void SetCells()
        {
            int childCount = rectChildren.Count;
            if (childCount == 0 || _childSizes == null) return;

            // Build cumulative offsets for columns/rows
            float[] colOffsets = new float[columns];
            float[] rowOffsets = new float[rows];

            float parentW = rectTransform.rect.width;
            float parentH = rectTransform.rect.height;

            float contentW = padding.left + padding.right + Sum(_colMax) + spacing.x * (columns - 1);
            float contentH = padding.top + padding.bottom + Sum(_rowMax) + spacing.y * (rows - 1);

            float extraW = Mathf.Max(0f, parentW - contentW);
            float extraH = Mathf.Max(0f, parentH - contentH);

            // childAlignment is TextAnchor (UpperLeft, MiddleCenter, etc.)
            float alignX = GetAlignX(childAlignment); // 0=left, 0.5=center, 1=right
            float alignY = GetAlignY(childAlignment); // 0=top, 0.5=middle, 1=bottom

            float startX = padding.left + extraW * alignX;
            float startY = padding.top + extraH * alignY;


            colOffsets[0] = startX;
            for (int c = 1; c < columns; c++)
                colOffsets[c] = colOffsets[c - 1] + _colMax[c - 1] + spacing.x;

            rowOffsets[0] = startY;
            for (int r = 1; r < rows; r++)
                rowOffsets[r] = rowOffsets[r - 1] + _rowMax[r - 1] + spacing.y;

            for (int i = 0; i < childCount; i++)
            {
                var child = rectChildren[i];
                int row = i / columns;
                int col = i % columns;

                float xPos = colOffsets[col];
                float yPos = rowOffsets[row];

                Vector2 sz = _childSizes[i];

                SetChildAlongAxis(child, 0, xPos, sz.x);
                SetChildAlongAxis(child, 1, yPos, sz.y);
            }
        }

        // helpers
        private void EnsureCaches(int childCount, int cols, int rowsCount)
        {
            if (_childSizes == null || _childSizes.Length != childCount)
                _childSizes = new Vector2[childCount];

            if (_colMax == null || _colMax.Length != cols)
                _colMax = new float[cols];
            else
                System.Array.Clear(_colMax, 0, _colMax.Length);

            if (_rowMax == null || _rowMax.Length != rowsCount)
                _rowMax = new float[rowsCount];
            else
                System.Array.Clear(_rowMax, 0, _rowMax.Length);
        }

        private static float Sum(float[] a)
        {
            float s = 0f;
            for (int i = 0; i < a.Length; i++) s += a[i];
            return s;
        }

        private static float GetAlignX(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.LowerLeft: return 0f;

                case TextAnchor.UpperCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.LowerCenter: return 0.5f;

                default: return 1f; // Right
            }
        }

        private static float GetAlignY(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight: return 0f; // top

                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight: return 0.5f;

                default: return 1f; // bottom
            }
        }
    }
}

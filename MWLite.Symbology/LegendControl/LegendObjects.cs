

using System.Drawing;
using MWLite.Symbology.Classes;

/// <summary>
/// һЩ����
/// </summary>
namespace MWLite.Symbology.LegendControl
{
	/// <summary>
	/// ����ͼ��ɼ���
	/// </summary>
	public enum VisibleStateEnum
	{
		/// <summary>
		/// ����ͼ��ɼ�
		/// </summary>
		vsALL_VISIBLE = 0,

		/// <summary>
		/// ����ͼ������
		/// </summary>
		vsALL_HIDDEN = 1,

		/// <summary>
		/// ���
		/// </summary>
		vsPARTIAL_VISIBLE = 2,
	}

	internal struct ColorInfo
	{
		#region ��Ա����

		public Color StartColor;
		public Color EndColor;
		public string Caption;
		public bool IsTransparent;

		#endregion

		public ColorInfo(Color  start,Color end, string pCaption, bool transparent)
		{
			StartColor = start;
			EndColor = end;
			Caption = pCaption;
			IsTransparent = transparent;
		}
		public ColorInfo(Color  start,Color end, string pCaption)
		{
			StartColor = start;
			EndColor = end;
			Caption = pCaption;
			IsTransparent = false;
			
		}
	}

	internal class DragInfo
	{
		#region ��Ա����
		public bool Dragging;
		public bool MouseDown;
		public bool LegendLocked;
		public int DragLayerIndex;
		public int DragGroupIndex;
		public int TargetGroupIndex;
		public int TargetLayerIndex;
		public int StartY;
		//public int StopY;
		#endregion

		public DragInfo()
		{
			Reset();
		}
		public void Reset()
		{
			Dragging = false;
			MouseDown = false;
			StartY = 0;
			LegendLocked = false;
			DragLayerIndex = -1;
			DragGroupIndex = -1;
			TargetGroupIndex = -1;
			TargetLayerIndex = -1;
		}

		public bool DraggingLayer
		{
			get
			{
				if(DragLayerIndex != -1)
					return true;
				else
					return false;
			}
		}
		public void StartGroupDrag(int MouseY, int GroupIndex)
		{
			MouseDown = true;
			DragGroupIndex = GroupIndex;
			DragLayerIndex = Constants.INVALID_INDEX;
			StartY = MouseY;
		}

		public void StartLayerDrag(int MouseY, int GroupIndex, int LayerIndex)
		{
			MouseDown = true;
			DragGroupIndex = GroupIndex;
			DragLayerIndex = LayerIndex;
			StartY = MouseY;

		}

//		private void StartDrag(int MouseY,int groupIndex, int LayerIndex)
//		{
//			MouseDown = true;
//			DragGroupIndex = groupIndex;
//			DragLayerIndex = LayerIndex;
//			StartY = MouseY;
//		}
	}
}

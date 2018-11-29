
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MWLite.Symbology.Classes;


namespace MWLite.Symbology.LegendControl
{
    //�������
    public class Group
	{
		#region ��Ա����

		private string m_Caption;

		/// <summary>
        /// Tag�����������ĳЩ��Ϣ
		/// </summary>
		public string Tag;
		private object m_Icon;
		private bool m_Expanded;
		private int m_Height;
		private Legend m_Legend;

		/// <summary>
		/// ��ľ��
		/// </summary>
		protected internal int m_Handle;

		/// <summary>
		/// ���е����λ
		/// </summary>
		protected internal int Top;

		/// <summary>
		/// ����Ĳ��б�
		/// </summary>
		public List<Layer> Layers;
		
		private VisibleStateEnum m_VisibleState;

        protected internal bool m_StateLocked;

        /// <summary>
        /// ����ͼ����Ϣ
        /// </summary>
        public string LayersGuidList()
        {
            return string.Join(";", Layers.Select(item => item.GuidKey + (item.Expanded ? "1" : "0")).ToArray());
        }
		#endregion

		/// <summary>
		/// ���캯��
		/// </summary>
		public Group(Legend leg)
		{
			
			m_Legend = leg;
			

			Layers = new List<Layer>();
			Expanded = true;
			VisibleState = VisibleStateEnum.vsALL_VISIBLE;
			m_Handle = -1;
			Icon = null;
            m_StateLocked = false;
		}
		/// <summary>
		/// ��������
		/// </summary>
		~Group()
		{
			m_Legend = null;
			Layers.Clear();
			Layers = null;
			m_Icon = null;
		}

		/// <summary>
		/// �ı�
		/// </summary>
		public string Text
		{
			get
			{
				return m_Caption;
			}
			set 
			{
				m_Caption = value;
				m_Legend.Redraw();
			}
		}

		/// <summary>
	    /// ͼ��
		/// </summary>
		public object Icon
		{
			get
			{
				return m_Icon;
			}
			set 
			{
				if(globals.IsSupportedPicture(value))
				{
					m_Icon = value;
					m_Legend.Redraw();
				}
				else
				{
					throw new System.Exception("Legend Error: Invalid Group Icon type");
				}
			}
		}

		/// <summary>
		/// ͼ����
		/// </summary>
		public int LayerCount
		{
			get
			{
				return Layers.Count;
			}
		}
		
		/// <summary>
		/// ����������ȡͼ��
		/// </summary>
		public Layer this[int LayerPosition]
		{
			get
			{
				if(LayerPosition >=0 && LayerPosition < this.Layers.Count)
					return (Layer)Layers[LayerPosition];

				globals.LastError = "Invalid Layer Position within Group";
					return null;
			}	
		}

		/// <summary>
		/// �����ľ��
		/// </summary>
		public int Handle
		{
			get
			{
				return m_Handle;
			}
		}

		/// <summary>
		/// ����������ȡͼ��
		/// </summary>
		/// <param name="Handle">ͼ����</param>
		protected internal Layer LayerByHandle(int Handle)
		{
			int count = Layers.Count;
			Layer lyr = null;
			for(int i = 0; i < count; i++)
			{
				lyr = (Layer)Layers[i];
				if (lyr.Handle == Handle)
					return lyr;
			}
			return null;
		}

		/// <summary>
		/// ��ȡͼ�������λ��
		/// </summary>
		/// <param name="Handle">ͼ����</param>
		/// <returns>0���ϣ�-1��ʾ����ʧ��</returns>
		protected internal int LayerPositionInGroup(int Handle)
		{
			int count = Layers.Count;
			Layer lyr = null;
			for(int i = 0; i < count; i++)
			{
				lyr = (Layer)Layers[i];
				if (lyr.Handle == Handle)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// ��ȡͼ��ͨ��λ��
		/// </summary>
		/// <param name="PositionInGroup">λ��</param>
		/// <returns>-1��ʾʧ��</returns>
		public int LayerHandle(int PositionInGroup)
		{
			if(PositionInGroup >=0 && PositionInGroup < Layers.Count)
				return ((Layer)Layers[PositionInGroup]).Handle;

			globals.LastError = "Invalid layer position within group";
			return -1;
		}

		/// <summary>
        /// �Ƿ�չ��
        /// </summary>
		public bool Expanded
		{
			get
			{
				return m_Expanded;
			}
			set
			{
				if(value != m_Expanded)
				{
					m_Expanded = value;
					RecalcHeight();
					m_Legend.Redraw();
				}
			}
		}

	    /// <summary>
        /// �߶�
        /// </summary>
		protected internal int Height
		{
			get
			{
                RecalcHeight();
				return m_Height;
			}
		}

        /// <summary>
        /// չ���߶�
        /// </summary>
		protected internal int ExpandedHeight
		{
			get
			{
				int NumLayers = Layers.Count;
				int Retval = Constants.ITEM_HEIGHT;
				Layer lyr;
                
				for(int i = 0; i < NumLayers; i++)
				{
					lyr = (Layer)Layers[i];
					Retval += lyr.CalcHeight(true);
				}
				

				return Retval;
			}
		}


		/// <summary>
		/// ���¼�����ĸ߶ȣ���ͼ�������ı�ʱ
		/// </summary>
		protected internal void RecalcHeight()
		{
			int NumLayers = Layers.Count;
			m_Height = Constants.ITEM_HEIGHT;
			Layer lyr;

			if(m_Expanded == true)
			{
				for(int i = 0; i < NumLayers; i++)
				{
					lyr = (Layer)Layers[i];
					if (!lyr.HideFromLegend)
						m_Height += lyr.Height;
				}
			}
			else
			{
				m_Height = Constants.ITEM_HEIGHT;
			}
		}


		public bool LayersVisible
		{
			get
			{
				if (VisibleState == VisibleStateEnum.vsALL_HIDDEN)
					return false;
				else
					return true;
			}
			set
			{
				if(value == true)
					VisibleState = VisibleStateEnum.vsALL_VISIBLE;
				else
					VisibleState = VisibleStateEnum.vsALL_HIDDEN;
			}
		}


		protected internal VisibleStateEnum VisibleState
		{
			get
			{
				return m_VisibleState;
			}
			set
			{
				if(value == VisibleStateEnum.vsPARTIAL_VISIBLE)
				{

					throw new System.Exception("Invalid [Property set] value: vsPARTIAL_VISIBLE");					
				}
				
				m_VisibleState = value;
				UpdateLayerVisibility();
			}
		}


        public bool StateLocked
        {
            get
            {
                return m_StateLocked;
            }
            set
            {
                m_StateLocked = value;
            }
        }


		private void UpdateLayerVisibility()
		{
			int NumLayers = Layers.Count;
			Layer lyr = null;
			bool visible = false;
			if (m_VisibleState == VisibleStateEnum.vsALL_VISIBLE)
				visible = true;

			for(int i = 0; i < NumLayers; i++)
			{
				lyr = (Layer)Layers[i];
				bool oldState = m_Legend.m_Map.get_LayerVisible(lyr.Handle);

				m_Legend.m_Map.set_LayerVisible(lyr.Handle,visible);				

				if (oldState != visible)
				{
					bool cancel = false;
					m_Legend.FireLayerVisibleChanged(lyr.Handle,visible, ref cancel);
					if (cancel == true)
						lyr.Visible = !(visible);
				}
			}
		}


		protected internal void UpdateGroupVisibility()
		{
			int NumVisible = 0;
			int NumLayers = Layers.Count;
			Layer lyr = null;
			for(int i = 0; i < NumLayers; i++)
			{
				lyr = (Layer)Layers[i];
				if(m_Legend.m_Map.get_LayerVisible(lyr.Handle) == true)
					NumVisible++;
			}

			if (NumVisible == NumLayers)
				m_VisibleState = VisibleStateEnum.vsALL_VISIBLE;
			else if (NumVisible == 0)
				m_VisibleState = VisibleStateEnum.vsALL_HIDDEN;
			else
				m_VisibleState = VisibleStateEnum.vsPARTIAL_VISIBLE;
		}

		/// <summary>
        /// �ṩ����
        /// </summary>
        /// <param name="imgWidth"></param>
        /// <returns></returns>
		public System.Drawing.Bitmap Snapshot(int imgWidth)
		{
			Bitmap bmp = null;// = new Bitmap(imgWidth,imgHeight);
			Rectangle rect;

			System.Drawing.Graphics g;
					
			bmp = new Bitmap(imgWidth,this.ExpandedHeight);
			g = Graphics.FromImage(bmp);
			g.Clear(System.Drawing.Color.White);

			rect = new Rectangle(0,0,imgWidth,this.ExpandedHeight);

			m_Legend.DrawGroup(g,this,rect,true);

			return bmp;
		}

        
        public SizeF MeasureCaption(Graphics g, Font font, int maxWidth)
        {
            return g.MeasureString(this.Text, font, maxWidth);
        }

        
        public SizeF MeasureCaption(Graphics g, Font font)
        {
            return g.MeasureString(this.Text, font);
        }
	}
}

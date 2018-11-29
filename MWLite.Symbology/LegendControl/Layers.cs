

using System.Collections.Generic;
using MWLite.Symbology.Classes;

namespace MWLite.Symbology.LegendControl
{
    /// <summary>
    /// ��Ĺ����࣬��������Legend�еĲ��������
    /// </summary>
	public class Layers: IEnumerable<Layer>
	{
		private Legend m_Legend;

        #region ���캯��
        public Layers(Legend leg)
		{
			//The next line MUST GO FIRST in the constructor
			m_Legend = leg;
			//The previous line MUST GO FIRST in the constructor
		}
        #endregion

        #region IEnumerable �ӿ�ʵ��
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<Layer> GetEnumerator()
        {
            for (int i = 0; i < Count; i++ )
            {
                var l = this[i];
                if (l == null)
                    break;
                yield return l;
            }
        }
        #endregion

        /// <summary>
        ///  ��������ͨ������б����
        /// </summary>
        /// <param name = "position">������0��ʼ</param>
        /// <returns>The layer</returns>
        public Layer this[int Position]
		{
			get
			{
				if(Position >=0 && Position < Count)	
				{
					int Handle = m_Legend.m_Map.get_LayerHandle(Position);
					return m_Legend.FindLayerByHandle(Handle);
				}

				globals.LastError = "Invalid Layer Position";
				return null;
			}
		}

        /// <summary>
		/// ��ȡ�������
		/// </summary>
		public int Count
		{
			get
			{
				if (m_Legend == null) return 0;
				if (m_Legend.m_Map == null) return 0;
				return m_Legend.m_Map.NumLayers;
			}
		}

		/// <summary>
		/// Get a Layer by the handle to the layer (without knowing what group the layer is in)
		/// </summary>
		/// <param name="Handle">The handle to the layer</param>
		/// <returns>Layer class item, null (nothing) on success</returns>
		public Layer ItemByHandle(int Handle)
		{
			return m_Legend.FindLayerByHandle(Handle);
		}


		/// <summary>
		/// Move a layer to a specified location within a specified group
		/// </summary>
		/// <param name="LayerHandle">Handle to the layer to move</param>
		/// <param name="TargetGroupHandle">Handle of the group into which to move the layer</param>
		/// <param name="PositionInGroup">0-Based index into the list of layers within the Target Group</param>
		/// <returns>True on success, False otherwise</returns>
		public bool MoveLayer(int LayerHandle, int TargetGroupHandle, int PositionInGroup)
		{
			return m_Legend.MoveLayer(TargetGroupHandle,LayerHandle,PositionInGroup);
		}

		/// <summary>
		/// Clears all layers from the legend (from all groups)
		/// </summary>
		public void Clear()
		{
			m_Legend.ClearLayers();
		}

		/// <summary>
		/// Move a layer to a new position within a group
		/// </summary>
		/// <param name="LyrHandle">Handle to the layer to move</param>
		/// <param name="NewPosition">0-based index of the desired position</param>
		/// <returns>True on Success, False otherwise</returns>
		public bool MoveLayerWithinGroup(int LyrHandle, int NewPosition)
		{
			int GroupIndex,
				LayerIndex;
          
			if (m_Legend.FindLayerByHandle(LyrHandle, out GroupIndex, out LayerIndex) != null)
			{
				Group grp = (Group)m_Legend.m_AllGroups[GroupIndex];
				return m_Legend.MoveLayer(grp.Handle,LyrHandle,NewPosition);
			}

			globals.LastError = "Invalid Layer Handle";
			return false;			
		}

        /// <summary>
        /// ����ͼ�����һ��ͼ��,λ���ڵ�ǰѡ��Ĳ���Ϸ������ڲ��б�Ķ���
        /// </summary>
		public int Add(object newLayer, bool Visible)
		{
			return m_Legend.AddLayer(newLayer,Visible);
		}

        /// <summary>
        /// ����Ķ������һ��ͼ��
        /// </summary>
        /// <returns>����ͼ��ľ����-1���ʧ��
		public int Add(object newLayer, bool Visible, bool PlaceAboveCurrentlySelected)
		{
			return m_Legend.AddLayer(newLayer,Visible,PlaceAboveCurrentlySelected);
		}

        /// <summary>
        /// ����Ķ������һ��ͼ��
        /// </summary>
        /// <param name="legendVisible">����lengend���Ƿ�ɼ�</param>
        /// <param name="newLayer">���ϲ����͵Ķ���</param>
        /// <param name="mapVisible">����ӵĲ��Ƿ��ڵ�ͼ����ʾ</param>
		public int Add(object newLayer, bool Visible, int TargetGroupHandle)
		{
			return m_Legend.AddLayer(newLayer,Visible,TargetGroupHandle);
		}

        /// <summary>
        /// ������ض�λ�����һ��ͼ��
        /// </summary>
        public int Add(object newLayer, bool Visible, int TargetGroupHandle, int afterLayerHandle)
        {
            return m_Legend.AddLayer(newLayer, Visible, TargetGroupHandle, true, afterLayerHandle);
        }

        /// <summary>
        /// ����߲�����еĶ������ͼ��
        /// </summary>
        public int Add(bool LegendVisible, object newLayer, bool MapVisible)
        {
            return m_Legend.AddLayer(newLayer, MapVisible, -1, LegendVisible);
        }

		/// <summary>
		/// �Ƴ���
		/// </summary>
		/// <param name="LayerHandle">���Ƴ�ͼ��ľ��</param>
		/// <returns>bool</returns>
		public bool Remove(int LayerHandle)
		{
			return m_Legend.RemoveLayer(LayerHandle);
		}

		/// <summary>
        /// ����ͼ�������е�λ��
        /// </summary>
        /// <param name="LayerHandle"></param>
        /// <returns></returns>
		public int PositionInGroup(int LayerHandle)
		{
			int LayerIndex ,
				GroupIndex ;
			Layer lyr = m_Legend.FindLayerByHandle(LayerHandle,out GroupIndex, out LayerIndex);

			if(lyr != null)
				return LayerIndex;
			
			globals.LastError = "Invalid Layer Handle";
			return -1;
		}

		/// <summary>
		/// ���ذ���ĳ��ͼ�����
		/// </summary>
		/// <param name="LayerHandle">ͼ����</param>
		/// <returns>������-1��ʾʧ��</returns>
		public int GroupOf(int LayerHandle)
		{

			int LayerIndex,
				GroupIndex;
			Layer lyr = m_Legend.FindLayerByHandle(LayerHandle,out GroupIndex, out LayerIndex);

			if(lyr != null)
			{
                Group grp = (Group)m_Legend.m_AllGroups[GroupIndex];
				return grp.Handle;
			}
			
			globals.LastError = "Invalid Layer Handle";
			return -1;
		}

		/// <summary>
		/// ָʾĳ�����Ƿ����
		/// </summary>
		/// <param name="Handle">����</param>
		/// <returns>bool</returns>
		public bool IsValidHandle(int Handle)
		{
			if(m_Legend.m_Map.get_LayerPosition(Handle) >=0)
				return true;
			else
				return false;
		}


		/// <summary>
		/// Collapse ����ͼ��
		/// </summary>
		public void CollapseAll()
		{
			m_Legend.Lock();
			int i, count;

			count = Count;
			for( i = 0; i < count; i++)
				this[i].Expanded = false;
			m_Legend.Unlock();
		}

		/// <summary>
		///չ������ͼ��
		/// </summary>
		public void ExpandAll()
		{
			m_Legend.Lock();
			int i, count;

			count = Count;
			for( i = 0; i < count; i++)
				this[i].Expanded = true;
			m_Legend.Unlock();
		}

        
    }
}

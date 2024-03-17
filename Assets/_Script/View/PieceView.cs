using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PieceView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] private Image m_Image;
	[SerializeField] private Toggle m_SelectPiece;
	[SerializeField] private PiecesSprites m_PiecesSprites;

	private BoardView m_BoardView;
	private Piece m_PieceModel;
	private GraphicRaycaster m_Raycaster;

	private bool m_IsInteractable = false;

	private Vector3 m_InitPos;
	private Transform m_PrevParent;
	private bool m_IsDragging = false;
	private bool m_WasSelected = false;
	private bool m_HasMoved = false;
	private float m_CellHalfSize = 90;

	// Start is called before the first frame update
	private void Start()
	{
		m_SelectPiece.onValueChanged.AddListener(_OnSelect);
		m_Raycaster = m_Image.canvas.GetComponent<GraphicRaycaster>();
	}

	private void _OnSelect(bool iSelect)
	{
		m_BoardView.SelectPiece(iSelect ? m_PieceModel : null);
	}

	public void InitPiece(BoardView iBoard, Piece iPiece, ToggleGroup iGroup)
	{
		m_BoardView = iBoard;
		m_PieceModel = iPiece;
		m_SelectPiece.group = iGroup;
		UpdateView();
	}

	public void UpdateView()
	{
		m_SelectPiece.isOn = false;

		if(m_PieceModel == null)
		{
			m_Image.enabled = false;
			return;
		}

		m_Image.enabled = true;

		PieceType pieceType = m_PieceModel.GetPieceData().pieceType;
		m_Image.sprite = m_PiecesSprites.Assets.Find(x => x.Piece == pieceType).Sprite;
		transform.localRotation = m_PieceModel.GetPlayerOwnership() == PlayerOwnership.BOTTOM ? Quaternion.identity : Quaternion.Euler(0, 0, 180);
	}

	public void SetInteractable(bool iIsInteractable)
	{
		m_IsInteractable = iIsInteractable;
		m_SelectPiece.enabled = iIsInteractable;
		m_SelectPiece.isOn = false;
	}

	public void OnPointerDown(PointerEventData _)
	{
		if(!m_IsInteractable)
			return;

		m_InitPos = transform.position;
		m_PrevParent = transform.parent;
		transform.SetParent(m_Image.canvas.transform, true);
		m_IsDragging = true;
		m_HasMoved = false;
		m_WasSelected = m_SelectPiece.isOn;
		m_SelectPiece.isOn = true;
	}

	private void Update()
	{
		if(!m_IsDragging)
			return;

		Vector3 pos = Input.mousePosition;
		pos.z = -100;
		transform.position = pos;

		Vector3 delta = pos - m_InitPos;
		if(Mathf.Abs(delta.x) > m_CellHalfSize || Mathf.Abs(delta.y) > m_CellHalfSize)
			m_HasMoved = true;
	}

	public void OnPointerUp(PointerEventData iMouseEventData)
	{
		if(!m_IsDragging)
			return;

		m_IsDragging = false;
		transform.SetParent(m_PrevParent, true);
		transform.position = m_InitPos;

		List<RaycastResult> hits = new List<RaycastResult>();
		m_Raycaster.Raycast(iMouseEventData, hits);
		foreach(RaycastResult hit in hits)
		{
			if(!hit.isValid)
				continue;

			CellView cell = hit.gameObject.GetComponent<CellView>();
			if(cell != null && cell.IsInteractable())
			{
				cell.Move();
				return;
			}
		}

		if(m_HasMoved)
			m_SelectPiece.isOn = false;
		else
			m_SelectPiece.isOn = !m_WasSelected;
	}
}

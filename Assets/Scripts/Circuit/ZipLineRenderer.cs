using UnityEngine;

namespace Circuit
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    public class ZipLineRenderer : MonoBehaviour
    {
        private LineRenderer _line;
        [SerializeField]private Transform point1;
        [SerializeField]private Transform point2;
        private Vector3 _point1Pos;
        private Vector3 _point2Pos;
        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _point1Pos = point1.localPosition;
            _point2Pos = point2.localPosition;
            _line.SetPosition(0, _point1Pos);
            _line.SetPosition(1, _point2Pos);
        }

        private void Update()
        {
        
            if (_point1Pos != point1.localPosition || _point2Pos != point2.localPosition)
            {
                _line.SetPosition(0, _point1Pos);
                _line.SetPosition(1, _point2Pos);
            }

            _point1Pos = point1.localPosition;
            _point2Pos = point2.localPosition;
        }
    }
}

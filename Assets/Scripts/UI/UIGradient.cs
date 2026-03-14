using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Horizontal Gradient Pro")]
public class UIGradient : BaseMeshEffect
{
    [Tooltip("Preset gradient color")]
    public Gradient effectGradient;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || effectGradient == null) return;

        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);

        if (vertices.Count == 0) return;

        // Tìm tọa độ X để xác định chiều ngang
        float leftX = vertices[0].position.x;
        float rightX = vertices[0].position.x;

        for (int i = 1; i < vertices.Count; i++)
        {
            float x = vertices[i].position.x;
            if (x > rightX) rightX = x;
            else if (x < leftX) leftX = x;
        }

        float width = rightX - leftX;

        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex v = vertices[i];

            // Tính toán tỉ lệ vị trí từ 0 (Trái) đến 1 (Phải)
            // Nếu width = 0 (trường hợp hiếm) thì trả về 0
            float ratio = (width > 0) ? (v.position.x - leftX) / width : 0;

            // Lấy màu chính xác từ dải Gradient tại vị trí ratio
            v.color = effectGradient.Evaluate(ratio);

            vertices[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }
}
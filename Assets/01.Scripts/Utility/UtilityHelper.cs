using Unity.VisualScripting;
using UnityEngine;

public static class UtilityHelper
{
    /// <summary>
    /// 예외처리된 특정 자식 이름의 컴포넌트를 가져오는 메서드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_this"></param>
    /// <param name="_childName"></param>
    /// <returns></returns>
    public static T TryGetChildComponent<T>(this MonoBehaviour _this, string _childName) where T : class
    {
        var child = UtilityHelper.FindChild(_this.transform, _childName);
        if (child == null) return null;

        T component = null;

        if (child.TryGetComponent<T>(out var findComponent)) component = findComponent;
        else LogHelper.Log($"{child.name}에 {typeof(T).Name}이라는 컴포넌트는 존재하지 않음");

        return component;
    }

    /// <summary>
    /// 예외처리가된 특정 자식 오브젝트의 컴포넌트를 가져오는  메서드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_parent"></param>
    /// <param name="_childName"></param>
    /// <returns></returns>
    public static T TryGetChildComponent<T>(this MonoBehaviour _this) where T : class
    {
        var component = _this.GetComponentInChildren<T>(true);
        if (component == null) LogHelper.Log($"{_this.name}에 {typeof(T).Name}이라는 컴포넌트는 존재하지 않음");

        return component;
    }

    /// <summary>
    /// 예외처리가 된 자기 자신 컴포넌트를 가져오는 메서드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_this"></param>
    /// <returns></returns>
    public static T TryGetComponent<T>(this MonoBehaviour _this)
    {
        if (_this.TryGetComponent<T>(out var component)) return component;
        else LogHelper.Log($"{_this.name}에 {typeof(T).Name}이라는 컴포넌트는 존재하지 않음");

        return default(T);
    }

    /// <summary>
    /// 특정 이름의 자식 오브젝트를 반환
    /// </summary>
    /// <param name="_parent"></param>
    /// <param name="_childName"></param>
    /// <returns></returns>
    public static GameObject TryFindChild(this MonoBehaviour _parent, string _childName)
    {
        var child = UtilityHelper.FindChild(_parent.transform, _childName);
        if (child == null) LogHelper.Log($"{_parent.name}에 {_childName}이라는 자식 오브젝트는 존재하지 않음");

        return child;
    }

    private static GameObject FindChild(Transform _parent, string _childName)
    {
        GameObject findChild = null;

        for (int i = 0; i < _parent.transform.childCount; i++)
        {
            var child = _parent.transform.GetChild(i);
            findChild = child.name == _childName ? child.gameObject : FindChild(child, _childName);
            if (findChild != null) break;
        }

        return findChild;
    }

    /// <summary>
    /// 제일 최상단 부모 오브젝트를 반환함
    /// </summary>
    /// <param name="_this"></param>
    /// <returns></returns>
    public static Transform TryFindParent(this Transform _this)
    {
        var parent = _this.transform.parent;

        if (parent == null) return _this.transform;
        else return parent.TryFindParent();
    }
}

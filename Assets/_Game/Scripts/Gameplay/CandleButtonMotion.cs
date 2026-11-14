using UnityEngine;
using UnityEngine.EventSystems;

public sealed class CandleButtonMotion:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler
{
    bool hover;
    public void OnPointerEnter(PointerEventData e)=>hover=true;
    public void OnPointerExit(PointerEventData e)=>hover=false;
    public void OnSelect(BaseEventData e)=>hover=true;
    public void OnDeselect(BaseEventData e)=>hover=false;
    void Update(){transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*(hover&&!CandlePresentation.ReducedMotion?1.025f:1),Time.unscaledDeltaTime*14);}
    void OnDisable(){hover=false;transform.localScale=Vector3.one;}
}


using UnityEngine;
using Zenject;

public class ZenjectMonoInstaller : MonoInstaller
{
    [SerializeField] private CanvasInputManager canvasInputManager;
    [SerializeField] private DrawingManager drawingManager;
    public override void InstallBindings()
    {
        Container.Bind<ICanvasInputManager>().To<CanvasInputManager>().FromInstance(canvasInputManager);
        Container.Bind<DrawingManager>().FromInstance(drawingManager);
        Container.Bind<IDrawingTool>().To<PencilDrawingTool>().AsSingle();
        Container.Bind<IDrawingTool>().To<PaintBucketDrawingTool>().AsSingle();
    }
}
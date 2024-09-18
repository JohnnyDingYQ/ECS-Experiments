using Unity.Collections;
using Unity.Entities;

public static partial class Factory
{
    public static Graph CreateGraph(EntityManager entityManager)
    {
        EntityCommandBuffer ecb = new(Allocator.Temp);
        Entity entity = ecb.CreateEntity();
        ecb.AddBuffer<Vertex>(entity);

        Entity store = entityManager.CreateEntity();
        ecb.AddComponent(store, new EntityStore() {entity = entity});

        ecb.Playback(entityManager);
        ecb.Dispose();

        return entityManager.GetAspect<Graph>(entityManager.GetComponentData<EntityStore>(store).entity);
    }
}
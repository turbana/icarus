using Unity.Entities;

namespace Icarus.Misc {
    // This tag should NEVER be added to any Entity. It is used to suppress the
    // automatic generation of entity queries by trying to match a component no
    // Entity should have.
    public struct NeverMatchTag : IComponentData {}

    /* Entities with a ReParent tag should have their Parent component re-added
     * at runtime. */
    public partial struct ReParent : IComponentData {
        public Entity Value;
    }

}

using Unity.Collections;
using Unity.Entities;

namespace Junk.Entities
{
    [DisableAutoCreation]
    public partial class TokenPoolSystem : SystemBase
    {
        //private EntityManager EntityManager;
        
        private EntityArchetype LightTokenArchetype;
        private EntityArchetype HeavyTokenArchetype;
        
        private int lightTokenCount = 1;
        private EntityQuery TokenGroup;
        
        private NativeArray<Token> TokenData;
        private NativeArray<Entity> Tokens;

        protected override void OnCreate()
        {
            LightTokenArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Token>(), ComponentType.ReadWrite<LightAttack>());
            HeavyTokenArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Token>(), ComponentType.ReadWrite<HeavyAttack>());
            
            for (int i = 0; i < lightTokenCount; i++)
            {
                EntityManager.CreateEntity(LightTokenArchetype);
            }
            EntityManager.CreateEntity(HeavyTokenArchetype);
            
            EntityQueryDesc query = new EntityQueryDesc
            {
                Any = new ComponentType[]{ typeof(Token) },
                None = new ComponentType[]{ typeof(Cooldown) }
            };
            TokenGroup = GetEntityQuery(query);
        }

        protected override void OnDestroy()
        {
            //TokenData.Dispose();
            //Tokens.Dispose();
        }

        public bool GetToken(Entity Requester)
        {
            if (TokenData.Length <= 0)
                return false;
            
            for (int i = 0; i < TokenData.Length; i++)
            {
                if (TokenData[i].User.Equals(Entity.Null))
                {
                    EntityManager.SetComponentData(Tokens[i], new Token
                    {
                        User = Requester
                    });
                    
                    if(EntityManager.HasComponent(Tokens[i], typeof(LightAttack)))
                        EntityManager.AddComponentData(Requester, new LightAttack());
                    
                    if(EntityManager.HasComponent(Tokens[i], typeof(HeavyAttack)))
                        EntityManager.AddComponentData(Requester, new HeavyAttack());
                    
                    return true;
                    //break;
                }
            }
            
            
            return false;
        }

        public void ReturnToken(Entity Requester)
        {
            for (int i = 0; i < TokenData.Length; i++)
            {
                if (TokenData[i].User.Equals(Requester))
                {
                    EntityManager.SetComponentData(Tokens[i], new Token());
                    break;
                }
            }
        }

        protected override void OnUpdate()
        {
            
        }
    }


    
    public struct Token : IComponentData
    {
        public Entity User;
    }
    
    public struct LightAttack : IComponentData
    {
    }
    
    public struct HeavyAttack : IComponentData
    {
    }
}
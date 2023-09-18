using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Junk.LineRenderer
{
    /// <summary>
    /// Forwards changes in <see cref="LineStyle"/> component to <see cref="RenderMesh"/> component.
    /// NOTE: System is started 100% manually until it's performace is sorted out.
    ///       world.GetOrCreateSystem<LineStyleUpdateSystem>().Update();
    /// </summary>
    //[WorldSystemFilter( 0 )]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [DisableAutoCreation]
    public partial class LineStyleUpdateSystem : SystemBase
    {
        
        EntityQuery query;
        List<LineStyle> _styles = new List<LineStyle>(10);

        protected override void OnCreate ()
        {
            query = EntityManager.CreateEntityQuery(
                    ComponentType.ReadOnly<LineStyle>()
                ,   ComponentType.ReadWrite<RenderMesh>()
            );
        }

        protected override void OnUpdate ()
        {
            var command = EntityManager;

            _styles.Clear();
            command.GetAllUniqueSharedComponentsManaged( _styles );

            var mesh = Internal.MeshProvider.lineMesh;
			int numStyles = _styles.Count;
            for( int i=0 ; i<numStyles ; i++ )
            {
				var style = _styles[i];
                query.SetSharedComponentFilterManaged( style );
                command.SetSharedComponentManaged( query , new RenderMesh{
                    mesh        = mesh ,
                    material    = style.material
                } );
            }
        }
    }
}

using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Junk.Core.Creation
{
    /// <summary>
    /// An audio clip representation accessible in Burst jobs.
    /// </summary>
    public struct BlobAudio
    {
        public int              channels;
        public int              frequency;
        public float            length;
        public int              samples;
        public BlobArray<float> sampleData;
        public BlobString       name;

        public static BlobAssetReference<BlobAudio> ToBlobData(AudioClip clip)
        {
            var     builder = new BlobBuilder(Allocator.Temp);
            ref var root     = ref builder.ConstructRoot<BlobAudio>();
            
            root.channels = clip.channels;
            root.frequency = clip.frequency;
            root.length = clip.length;
            root.samples = clip.samples;
            
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            /*var nativeSamples = new NativeArray<float>(samples, Allocator.Temp);
            nativeSamples.CopyFrom(samples);*/
            
            var sampleDataArray = builder.Allocate<float>(ref root.sampleData, samples.Length);
            for (int i = 0; i < sampleDataArray.Length; i++)
                sampleDataArray[i] = samples[i];

            builder.AllocateString(ref root.name, clip.name);

            var blob = builder.CreateBlobAssetReference<BlobAudio>(Allocator.Persistent);
            root = ref blob.Value;
            
            builder.Dispose();

            return blob;
        }
    }
}
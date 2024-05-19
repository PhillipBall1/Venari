using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipData", menuName = "System/Audio/AudioClipData")]
public class AudioClipData : ScriptableObject
{
    public AudioClipGroup[] audioClipGroups;
    private Dictionary<string, AudioGroupProperties> audioClipGroupDictionary;

    [System.Serializable]
    public struct AudioClipGroup
    {
        public string groupName;
        public AudioClip[] audioClips;
        public SoundLevel defaultSoundLevel; // Default sound level for this group
    }

    [System.Serializable]
    public enum SoundLevel
    {
        Loud,
        Regular,
        Soft,
        Quiet
    }

    private void OnEnable()
    {
        audioClipGroupDictionary = new Dictionary<string, AudioGroupProperties>();
        if (audioClipGroups == null) return;
        foreach (var group in audioClipGroups)
        {
            audioClipGroupDictionary[group.groupName] = new AudioGroupProperties
            {
                audioClips = group.audioClips,
                defaultSoundLevel = group.defaultSoundLevel
            };
        }
    }

    public AudioClip[] GetClipsForGroup(string groupName)
    {
        if (audioClipGroupDictionary.TryGetValue(groupName, out AudioGroupProperties properties))
        {
            return properties.audioClips;
        }
        return null;
    }

    public SoundLevel GetSoundLevelForGroup(string groupName)
    {
        if (audioClipGroupDictionary.TryGetValue(groupName, out AudioGroupProperties properties))
        {
            return properties.defaultSoundLevel;
        }
        return SoundLevel.Regular; // Default sound level
    }

    private struct AudioGroupProperties
    {
        public AudioClip[] audioClips;
        public float defaultPitchMin;
        public float defaultPitchMax;
        public SoundLevel defaultSoundLevel;
    }

    public void AddAudioClipArray(string groupName, AudioClip[] newClips, SoundLevel soundLevel = SoundLevel.Regular)
    {
        if (audioClipGroupDictionary.ContainsKey(groupName))
        {
            var existingGroup = audioClipGroupDictionary[groupName];
            var updatedClipsList = new List<AudioClip>(existingGroup.audioClips);
            updatedClipsList.AddRange(newClips);
            existingGroup.audioClips = updatedClipsList.ToArray();
            audioClipGroupDictionary[groupName] = existingGroup;
        }
        else
        {
            var newGroup = new AudioGroupProperties  
            {
                audioClips = newClips,
                defaultSoundLevel = soundLevel
            };
            audioClipGroupDictionary.Add(groupName, newGroup);

            var groupsList = new List<AudioClipGroup>(audioClipGroups);
            groupsList.Add(new AudioClipGroup { groupName = groupName, audioClips = newClips, defaultSoundLevel = soundLevel });
            audioClipGroups = groupsList.ToArray();
        }
    }

    public void AddGroup(string groupName, SoundLevel soundLevel = SoundLevel.Regular)
    {
        if (audioClipGroupDictionary.ContainsKey(groupName))
        {
            Debug.LogWarning("Group already exists with the name: " + groupName);
            return;
        }

        var newGroup = new AudioGroupProperties
        {
            audioClips = new AudioClip[0], // Initialize with an empty array
            defaultSoundLevel = soundLevel
        };
        audioClipGroupDictionary.Add(groupName, newGroup);

        // Initialize audioClipGroups if it's null
        if (audioClipGroups == null)
        {
            audioClipGroups = new AudioClipGroup[0];
        }

        var groupsList = new List<AudioClipGroup>(audioClipGroups);
        groupsList.Add(new AudioClipGroup { groupName = groupName, audioClips = new AudioClip[0], defaultSoundLevel = soundLevel });
        audioClipGroups = groupsList.ToArray();
    }


}

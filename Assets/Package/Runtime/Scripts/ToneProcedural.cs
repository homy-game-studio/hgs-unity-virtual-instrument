using System.Collections.Generic;
using UnityEngine;

namespace HGS.Tone
{
  public class ToneProcedural : MonoBehaviour
  {
    [SerializeField] ToneSynth synth = null;

    ToneChordStyle _chordStyle = null;
    List<ToneChord> _chords = new List<ToneChord>();
    ToneChord _currentChord;
    List<ToneNote> _currentBassNotes;
    int _currentChordIndex = -1;

    bool _isPlaying = false;

    int _beatCount = 0;
    float _beatTimer = 0;
    float _beatDuration = 0.15f;
    float _tempo = 8;

    public void Stop()
    {
      _currentChordIndex = -1;
      _beatCount = 0;
      _beatTimer = 0;
      _isPlaying = false;
      synth.TriggerReleaseAll();
    }

    public void Generate()
    {
      _chords = ToneChordProgression.Random().Generate(ToneNote.Random());
      _chordStyle = ToneChordStyle.Random();
    }

    public void Play()
    {
      _isPlaying = true;
      ProcessBeat();
    }

    void ProcessStyleNotation(int id, string notation, List<ToneNote> notes)
    {
      if (id >= notes.Count) return;

      var note = notes[id];

      switch (notation)
      {
        case "_": synth.TriggerAttack(note); break;
        case "x": synth.TriggerRelease(note); break;
        case ".": synth.TriggerAttackAndRelease(note, duration: _beatDuration / 2f); break;
      }
    }

    void ProcessBeat()
    {
      var beat = (int)(_beatCount % _tempo);
      var canChangeNote = beat == 0;

      if (canChangeNote)
      {
        synth.TriggerReleaseAll();

        _currentChordIndex++;

        if (_currentChordIndex == _chords.Count) _currentChordIndex = 0;

        _currentChord = _chords[_currentChordIndex];
        _currentBassNotes = new List<ToneNote>{
            new ToneNote(_currentChord.BaseNote.Semitones).RemoveOctaves(2),
            new ToneNote(_currentChord.BaseNote.Semitones).RemoveOctaves(2).AddSemitones(7),
            new ToneNote(_currentChord.BaseNote.Semitones).RemoveOctaves(1),
        };
      }

      var bass = _chordStyle.GetBass(beat);
      var chord = _chordStyle.GetChord(beat);

      for (int i = 0; i < chord.Length; i++) ProcessStyleNotation(i, chord[i], _currentChord.Notes);
      for (int i = 0; i < bass.Length; i++) ProcessStyleNotation(i, bass[i], _currentBassNotes);

      Debug.Log($"Chord: {_currentChord.ToString()}");

      _beatCount += 1;
    }

    void Update()
    {
      if (Input.GetKeyDown(KeyCode.Return))
      {
        Stop();
        Generate();
        Play();
      }

      if (_isPlaying)
      {
        _beatTimer += Time.deltaTime;
        if (_beatTimer >= _beatDuration)
        {
          _beatTimer = 0;
          ProcessBeat();
        }
      }
    }
  }
}
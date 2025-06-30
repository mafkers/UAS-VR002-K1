// Music_Player.cs (Versi Modifikasi dengan DontDestroyOnLoad & Singleton)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Music_Player : MonoBehaviour {

    // --- BAGIAN TAMBAHAN UNTUK SINGLETON ---
    public static Music_Player instance;
    // -----------------------------------------

    AudioSource _audioSource;
    public AudioClip[] _audioTracks;
    [Space(20)]
    [HeaderAttribute("Music Player Options")]
    public bool _playTracks;
    public bool _nextTrack;
    public bool _prevTrack;
    public bool _loopTrack;
    [Space(20)]
    [HeaderAttribute("Debugging/ReadOnly")]
    public int _playingTrack;
    public bool _isMute = false;
    
    void Awake () {
        // --- LOGIKA SINGLETON DITAMBAHKAN DI SINI ---
        if (instance == null)
        {
            // Jika belum ada instance Music_Player, jadikan ini sebagai instance utama
            instance = this;
            // Dan jangan hancurkan object ini saat pindah scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Jika sudah ada instance Music_Player lain, hancurkan object baru ini
            Destroy(gameObject);
            return; // Hentikan eksekusi sisa Awake()
        }
        // -----------------------------------------------
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioTracks.Length > 0)
        {
            _audioSource.clip = _audioTracks[0];
        }
        _playingTrack = 0;
        
        if(PlayerPrefs.HasKey("_Mute")){
            int _value = PlayerPrefs.GetInt("_Mute");
            if(_value == 0){_isMute = false;}
            if(_value == 1){_isMute = true;}
        } else {
            _isMute = false;
            PlayerPrefs.SetInt("_Mute", 0);
        }

        if( _isMute ){ _audioSource.mute = true; } 
        else 
        { 
            _audioSource.mute = false;
            // Hanya putar jika playTracks di-set true
            if (_playTracks) _audioSource.Play();
        }
    }
    
    void Update () {
        if(!_playTracks) _audioSource.Stop();
        if(_playTracks && !_audioSource.isPlaying && _audioTracks.Length > 0) StartPlayer();          
        if(_loopTrack){ _audioSource.loop = true; } else { _audioSource.loop = false; }
        if(_nextTrack){ NextTrack(); }
        if(_prevTrack){ PreviousTrack(); }
        
        int _value = PlayerPrefs.GetInt("_Mute");
        if(_value == 0){_isMute = false;}
        if(_value == 1){_isMute = true;}
        if( _isMute ){ _audioSource.mute = true; } else { _audioSource.mute = false; }
    }
    
    public void StartPlayer(){
        if(!_loopTrack) {
            NextTrack();
        } else {
            _audioSource.Play(); 
        }
    }
    
    public void NextTrack(){
        if (_audioTracks.Length == 0) return;
        _nextTrack = false;
        _audioSource.Stop();
        int _newCount = _playingTrack+1;
        if(_newCount > _audioTracks.Length-1) {
            _audioSource.clip = _audioTracks[0]; _playingTrack = 0;
        } else {
            _audioSource.clip = _audioTracks[_newCount];_playingTrack = _newCount;
        } 
        _audioSource.Play();
    }
    
    public void PreviousTrack(){
        if (_audioTracks.Length == 0) return;
        _prevTrack = false;
        _audioSource.Stop();
        int _newCount = _playingTrack-1;
        if(_newCount < 0) {
            _audioSource.clip = _audioTracks[_audioTracks.Length-1]; 
            _playingTrack = _audioTracks.Length-1;
        } else {
            _audioSource.clip = _audioTracks[_newCount];
            _playingTrack = _newCount;
        }
        _audioSource.Play();
    }
}
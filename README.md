# Conversational AI NPC — Unity VR Prototype

Unity VR prototype featuring a conversational NPC powered by a full voice pipeline: STT (Whisper), LLM (Ollama/OpenAI), and TTS (ElevenLabs) with lip sync.

Built in 4 days as a technical prototype for a VR experience featuring an interactive historical figure.

---

## Demo

> speak to Roger de Flor, a 13th-century Catalan admiral and leader of the Almogavars — ask him about his battles, his life, and his death.

---

## Pipeline

```
Microphone input
      ↓
STT — Whisper (local via whisper.unity / OpenAI Whisper API)
      ↓
LLM — Ollama with LLama 3.2 (local / OpenAI GPT-4o mini API)
      ↓
TTS — ElevenLabs API
      ↓
NPC audio playback + OVR Lip Sync
```

---

## Architecture

The system is built around a **decoupled event-driven architecture**:

- **ConversationEventSystem** — central event bus with typed `UnityEvent<T>` events
- **ConversationStateMachine** — FSM that controls valid state transitions and prevents concurrency issues
- Each service (STT, LLM, TTS) is fully independent — it listens to its input event and emits its output event
- Switching between local and API implementations requires only toggling a component in the Inspector

### State flow
```
Idle → Recording → Transcribing → WaitingLLM → Speaking → Idle
```

### Scripts
```
ConversationSystem/
├── Events/
│   └── ConversationEventSystem.cs
├── StateMachine/
│   ├── ConversationStateMachine.cs
│   └── ConversationStates.cs
├── STT/
│   ├── VoiceInputHandler.cs
│   ├── WhisperLocalSTT.cs
│   └── WhisperAPISTT.cs
├── LLM/
│   ├── OllamaLLMService.cs
│   └── OpenAILLMService.cs
├── TTS/
│   └── ElevenLabsTTS.cs
├── NPC/
│   └── LipSyncHelper.cs
└── Data/
    ├── CharacterData.cs
    └── APIConfig.cs
```

---

## Character System

Characters are defined via **ScriptableObjects** (`CharacterData`), making it trivial to swap the historical figure without touching any code:

- `characterName` — display name
- `systemPrompt` — full personality, era, speech style, and behavioral constraints

The LLM maintains a full **conversation history** per session, so the character remembers what has been discussed.

---

## Tech Stack

| Layer | Local | API |
|---|---|---|
| STT | whisper.unity (ggml-base) | OpenAI Whisper API |
| LLM | Ollama + LLama 3.2 | OpenAI GPT-4o mini |
| TTS | — | ElevenLabs |
| Lip Sync | OVR Lip Sync (Meta) | — |
| VR | Unity XR Interaction Toolkit + OpenXR | — |
| Input | Unity New Input System | — |

---

## Setup

### Requirements
- Unity 6 or later
- Meta Quest 2 / 3 (or PC VR)
- [Ollama](https://ollama.com) installed and running locally

### Installation

1. Clone the repo
2. Open in Unity
3. Install Ollama and pull the model:
```bash
ollama pull llama3.2
```
4. Download a Whisper model (`ggml-base.bin`) from [HuggingFace](https://huggingface.co/ggerganov/whisper.cpp/tree/main) and place it in `Assets/StreamingAssets/`
5. Create an `APIConfig` asset at `Assets/Resources/APIConfig.asset` and fill in your API keys (see below)

### API Keys

Create the `APIConfig` ScriptableObject via:
**Right click in Project → Create → Conversation → API Config**

Fill in:
- `elevenLabsKey` — from [elevenlabs.io](https://elevenlabs.io)
- `elevenLabsVoiceId` — voice ID from your ElevenLabs account
- `openAIKey` — from [platform.openai.com](https://platform.openai.com) (optional, for API mode)

> ⚠️ `APIConfig.asset` is in `.gitignore` — never commit your API keys.

---

## Key Design Decisions

**Event-driven architecture over direct references**
Services don't know about each other. This makes it trivial to swap implementations — disable `WhisperLocalSTT`, enable `WhisperAPISTT`, done.

**FSM for concurrency control**
Without state management, a user could trigger a new recording while the NPC is still speaking. The `ConversationStateMachine` blocks invalid transitions at the source.

**ScriptableObjects for character data**
Swapping the historical figure requires no code changes — just assign a different `CharacterData` asset in the Inspector.

**API key security**
All sensitive keys are stored in a ScriptableObject asset excluded from version control via `.gitignore`.

---

## License

MIT

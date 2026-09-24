# APGuardAI: Enterprise-Grade Deterministic RAG Architecture 

APGuardAI, B2B finansal operasyonlar ve ERP sistemleri için tasarlanmış, **sıfır halüsinasyon toleranslı**, kapalı devre bir yapay zeka mutabakat ve fatura denetim sistemidir. 

Geleneksel RAG (Retrieval-Augmented Generation) mimarilerindeki en büyük sorun olan LLM halüsinasyonları ve öngörülemeyen API maliyetleri, bu projede olasılıksal dil modellerinin **deterministik C# kural motorlarıyla (Rule Engines)** sınırlandırılmasıyla çözülmüştür.

##  Mimari Akış ve Güvenlik Duvarı

Aşağıdaki şema, sistemin olasılıksal bir LLM'i C# katmanında nasıl kısıtladığını ve denetlediğini göstermektedir:

```mermaid
graph TD
    %% Bileşenler
    A[ERP / Fatura Talebi] -->|İstek| B(C# Application Layer)
    
    subgraph Kognitif Yük Optimizasyonu
    B --> C{Opaque ID Mapper}
    C -->|Bağlam Bağımsız UUID'ler gizlenir| D[Geçici Kısa ID'ler Üretilir <br/> örn: ref_01]
    end
    
    subgraph Vektör Arama & Bağlam
    D --> E[(PostgreSQL + pgvector)]
    E -->|Cosine Similarity Barajı| F[Sadece İlgili Sözleşme Maddeleri]
    end
    
    subgraph LLM & Kısıtlamalar
    F --> G[Lokal LLM: Qwen 2.5 1.5B <br/> 4 GiB RAM Sınırı]
    G -.->|llama.cpp GBNF Grammar| G
    G -->|JSON Çıktısı| H
    end
    
    subgraph Deterministik Doğrulama
    H{C# Exact Quote Validator}
    H -->|Sözleşmede Karşılığı Yok| I[HATA: İşlem İptali <br/> QuoteMismatch]
    H -->|Format veya ID Hatalı| J[HATA: İşlem İptali <br/> InvalidFinancialReference]
    H -->|Byte-Byte Eşleşme Başarılı| K[Onay / Ret Kararı <br/> ExplanationDraft]
    end
    
    %% Renklendirme
    classDef secure fill:#e8f4f8,stroke:#0366d6,stroke-width:2px;
    classDef ai fill:#f3f0ff,stroke:#6f42c1,stroke-width:2px;
    classDef db fill:#f0fff4,stroke:#22863a,stroke-width:2px;
    classDef error fill:#ffeef0,stroke:#d73a49,stroke-width:2px;
    classDef success fill:#d4edda,stroke:#28a745,stroke-width:2px;
    
    class C,H secure;
    class G ai;
    class E db;
    class I,J error;
    class K success;

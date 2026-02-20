# Codex Instructions

This file defines mandatory architectural rules.
Any generated code must comply with these rules.
If a requested change violates these rules, explain why.

1. プロジェクトの目的
Minkaiは生活外部記憶OS
ローカルファースト
家族単位前提
2. アーキテクチャ原則
ViewModelはUIのみ
Serviceは業務ロジックのみ
Repositoryは永続化のみ
SQLiteAsyncConnectionはRepository内部のみ
3. 禁止事項
ViewModelから直接DBアクセス禁止
トランザクション境界の勝手な変更禁止
命名規則変更禁止
4. コーディング規約
Asyncサフィックス必須
DIはAddScoped
Interface必須
# Диаграммы

## Диаграмма базы данных

```mermaid
erDiagram
    categories ||--o{ products : ""
    categories ||--o{ brands : ""
    brands ||--o{ products : ""
    cities ||--o{ users : ""
    cities ||--o{ contacts : ""
    products ||--o{ products_log : ""

    categories {
        SERIAL id PK
        VARCHAR name UK
        INTEGER sort_order
        TIMESTAMP created_at
    }

    brands {
        SERIAL id PK
        VARCHAR name UK
        INTEGER category_id FK
    }

    products {
        SERIAL id PK
        INTEGER category_id FK
        INTEGER brand_id FK
        VARCHAR name
        TEXT description
        VARCHAR image_url
        DECIMAL price
        JSONB details
        TIMESTAMP created_at
        TIMESTAMP updated_at
        BOOLEAN is_active
    }

    products_log {
        SERIAL id PK
        INTEGER product_id FK
        VARCHAR action
        JSONB old_data
        JSONB new_data
        TIMESTAMP changed_at
    }

    cities {
        SERIAL id PK
        VARCHAR name UK
    }

    users {
        SERIAL id PK
        BIGINT tg_id UK
        VARCHAR first_name
        VARCHAR last_name
        VARCHAR username
        DATE birthday
        INTEGER city_id FK
        BOOLEAN is_verified
        BOOLEAN agreed_to_terms
        TIMESTAMP created_at
        TIMESTAMP last_activity
    }

    contacts {
        SERIAL id PK
        INTEGER city_id FK
        VARCHAR email
        JSONB social_links
        JSONB phones
        JSONB addresses
        TIMESTAMP updated_at
    }

    admins {
        SERIAL id PK
        VARCHAR login UK
        VARCHAR password_hash
        VARCHAR full_name
        BOOLEAN is_active
        BOOLEAN is_locked
        TIMESTAMP last_login
        TIMESTAMP created_at
        TIMESTAMP updated_at
        JSONB permissions
    }
```

<br>

## Логика взаимодействий

```mermaid
flowchart TB
    subgraph ADMINS["Администраторы"]
        direction TB
        A1[Админ 1]
        A2[Админ 2]
        A3[Админ 3]
        A4[Админ 4]
        A5[Админ 5]
    end

    subgraph SERVERS["Серверная часть"]
        DB[(Сервер БД)]
        BOT[Сервер бота]
    end

    OTHER[Бот / другой продукт]

    ADMINS <-->|Act.1| SERVERS
    SERVERS <-->|Act.2| OTHER
```

### Act. 1 - Взимодействие "Админ <-> Сервер"

1. Взимодействие происходит от имени администратора (таблица admins), ведется логирование.
  
2. Доступы настроены в формате JSON, так же ограничение внутри базы данных через GRANT.

3. Уровень взаимодействия - полный (Просмотр, внесение, удаление, изменение)

### Act. 2 - Взаимодействие "Сервер <-> Бот"

1. Взаимодействие присходит от лица Bot_user (Создание пользователя внутри базы данных, ограничение исключительно на GRANT SELECT (только просмотр))

2. Уровень взаимодействия - низкий

   Бот -> Cервер: исключительно SELECT;

   Сервер -> Бот: Полное предоставление данных для просмотра

<br>

## Описание взаимодействий

Админ -> (SELECT, INSERT, DELETE, UPDATE) ->

База данных -> (GRANT SELECT) ->

Бот -> (GRANT SELECT) ->

Конечный пользователь

<br>

## Определение доступа к программе и её функционалу

```mermaid
flowchart TB
    A[Ввод логина/пароля] --> B{Поля заполнены?}
    B -->|Нет| C[❌ Ошибка]
    B -->|Да| D{Аккаунт активен?}
    D -->|Нет| E[❌ Деактивирован]
    D -->|Да| F{Аккаунт заблокирован?}
    F -->|Да| G[🔒 Заблокирован]
    F -->|Нет| H{Пароль верный?}
    H -->|Нет| I{Попыток < 5?}
    I -->|Да| J[❌ Неверный пароль]
    I -->|Нет| K[🔒 Блокировка]
    H -->|Да| L[✅ Вход разрешен]
    
    J --> A
    K --> G
    L --> M[Загрузка интерфейса]
```

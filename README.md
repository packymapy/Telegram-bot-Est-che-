# Пробная версия бота для "ЕСТЬ ЧЁ?"

<br>
<br>
<br>

# Глава 1. Предметная область

<br>

## 1. Мнимая проверка возраста, Выбор города

### 1. Бот предлагает ввести дату рождения в формате дд.мм.гггг

### 2. Бот предлагает выбрать город (Сыктывкар / Киров)


======================================================================

***!!!*** ***После ввода даты рождения предупредить, что мы можем попросить паспорт при продаже*** ***!!!***

======================================================================

<br>

## 2. Показ основных кнопок (Каталог, Контакты, Профиль)
   
### 1. Каталог
   
#### 1. Жевательный табак
      
#### 2. Жидкости
      
#### 3. Жидкости без никотина
      
#### 4. Ароматизаторы
      
#### 5. Одноразовые устройства
       
#### 6. Табак для кальяна
       
#### 7. Расходные материалы
       
#### 8. ЭСДН
       
#### 9. Сигариллы и альтернатива
       
#### 10. Напитки и снэки
       
#### 11. Баки, моды и акссессуары
   
#### 12. Ликвидация.

=========================================================

***!!!*** &nbsp; ***На каждой категории должно быть краткое описание / предупреждение*** &nbsp; ***!!!***

=========================================================

<br>

### 2. Контакты
#### 1. Email
#### 2. Социальные сети
#### 3. Номера телефонов ***(по выбранному городу)***
#### 4. Адреса, часы работы ***(по выбранному городу)***

<br>

### 3. Профиль
#### 1. Имя
#### 2. Дата рождения ***(возраст определяется автоматически)*** ***(Сохраняется при первом взаимодействии с ботом)***
#### 3. Город ***(Сохраняется при первом взаимодействии с ботом)***

=========================================================

***!!!*** &nbsp; ***Дату рождения можно поменять (если указали с ошибкой)*** &nbsp; ***!!!***

***!!!*** &nbsp; ***Город можно поменять*** &nbsp; ***!!!***

=========================================================

<br>
<br>
<br>

# Глава 2. Определение СУБД и составление списка необходимых таблиц

<br>

## Выбор СУБД

### ***СУБД - Система управления базами данных.***

### Это программное обеспечение, выступающее посредником между программой и базой данных. Она отвечает за хранение, изменение, защиту информации, а также позволяет быстро находить и извлекать нужные данные.

### Ниже представлено сравнение двух самых популярных СУБД для работы внутри Telegram:

|Критерий | SQLite | PostgreSQL | Почему это критично для бота
|---------|--------|------------|-----------------------------
|Одновременные пользователи | При 10-20 одновременных запросах от разных юзеров может начать выдавать database is locked | Легко держит сотни и тысячи параллельных запросов | В боте все пользователи активны одновременно (оформляют заказы, смотрят каталог). SQLite начнёт тормозить раньше
|Работа через облако / хостинг | Файл .db должен лежать на постоянном диске. При деплое через Docker/сервер без persistence - данные теряются | Работает через сеть (TCP/IP). База на отдельном сервере, бот на другом - стандартная схема | ТГ-бота обычно хостят на облачных серверах (Railway, Render, AWS, Amvera cloud). PostgreSQL там - нативная поддержка, SQLite - головная боль
|Масштабирование: один бот → несколько | Файл БД нельзя расшарить между несколькими инстансами бота | Поддерживает репликацию, pooling (PgBouncer) | Если бот вырастет, вы захотите запустить 2-3 копии бота на нагрузку. SQLite это физически не позволяет
|Резервное копирование без остановки | Нужно останавливать бота или копировать файл «на живую» - риск повреждения | pg_dump или pg_basebackup без остановки бота | Бот работает 24/7. Вы не можете выключить бота, чтобы сделать бэкап
|Удалённое администрирование | Нет. Чтобы посмотреть таблицы, нужно заходить на сервер и открывать файл | Полноценный psql или PgAdmin из любой точки мира | Вы сидите дома, а сервер где-то в облаке. Через SQLite это неудобно, через Postgres - стандартно
|Безопасность данных пользователей | Только права на файл в ОС | Роли, пароли, шифрование соединений (SSL), RLS | В боте хранятся адреса и телефоны пользователей - утечка недопустима. SQLite не даст нормального разграничения
|Поиск по товарам / категориям | Полнотекстовый поиск есть, но слабый (FTS5) | Мощный pg_trgm, tsvector, частичные индексы | SQLite ищет по названию медленно. PostgreSQL - мгновенно

<br>

### Выбрана СУБД PostgreSQL как самый оптимальный и безопасный вариант хранения данных.

<br>

## Определение необходимых таблиц (товар)

### Таблицы и их данные

#### Таблица categories

|Поле | Тип | Ограничения | Описание
|-----|-----|-------------|---------
|id | SERIAL (INTEGER) | PRIMARY KEY | Первичный ключ
|name | VARCHAR(100) | NOT NULL, UNIQUE | Название категории
|sort_order | INTEGER | DEFAULT 0 | Порядок сортировки
|created_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Дата создания

<br> 

***Индексы: Автоматический индекс по PRIMARY KEY (id)***

<br>

#### Таблица products

|Поле | Тип | Ограничения | Описание
|-----|-----|-------------|---------
|id | SERIAL | PRIMARY KEY | Первичный ключ
|category_id | INTEGER | NOT NULL, FOREIGN KEY → categories(id) | Ссылка на категорию
|name | VARCHAR(255) | NOT NULL | Название товара
|price | DECIMAL(10,2) | NOT NULL | Цена
|details | JSONB | NOT NULL | Характеристики (JSONB)
|created_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Дата создания
|updated_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP (auto-update) | Дата изменения
|is_active | BOOLEAN | DEFAULT TRUE | Активен ли товар

<br>

***Индексы***

|Имя индекса | Поле(я) | Тип
|------------|-----------|---------
|idx_products_category | category_id | B-tree
|idx_products_active | is_active | B-tree
|idx_products_price | price | B-tree
|idx_products_details | details | GIN
|idx_products_name | name | B-tree

<br>

#### Таблица products_log

|Поле | Тип | Ограничения | Описание
|-----|-----|-------------|---------
|id | SERIAL | PRIMARY KEY | Первичный ключ
|product_id | INTEGER | NOT NULL, FOREIGN KEY → products(id) | Ссылка на товар
|action | VARCHAR(10) | CHECK (action IN (...)) | insert / update / delete
|old_data | JSONB | NULL | Данные до изменения
|new_data | JSONB | NULL | Данные после изменения
|changed_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Время изменения

<br>

***Индексы***

|Имя индекса | Поле(я) | Тип
|------------|---------|---------
|products_log_pkey | id | B-tree
|idx_product (авто) | product_id | B-tree
|idx_changed_at | changed_at | B-tree

<br>

### Запросы на создение таблиц

```sql
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
```

<br>

```sql
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    category_id INTEGER NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    name VARCHAR(255) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    details JSONB NOT NULL,  -- в PostgreSQL лучше JSONB (быстрее и можно индексировать)
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE);
```

<br>

```sql
CREATE TABLE products_log (
    id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    action VARCHAR(10) NOT NULL CHECK (action IN ('insert', 'update', 'delete')),
    old_data JSONB,
    new_data JSONB,
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
```

<br>

### Запросы на создание функций и триггеров 

```sql
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

<br>

```sql
CREATE TRIGGER trigger_products_updated_at
    BEFORE UPDATE ON products
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

<br>

```sql
CREATE OR REPLACE FUNCTION log_products_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO products_log (product_id, action, new_data)
    VALUES (
        NEW.id,
        'insert',
        jsonb_build_object('name', NEW.name, 'price', NEW.price, 'details', NEW.details));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

<br>

```sql
CREATE TRIGGER trigger_products_insert_log
    AFTER INSERT ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_insert();
```

<br>

```sql
CREATE OR REPLACE FUNCTION log_products_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO products_log (product_id, action, old_data, new_data)
    VALUES (
        NEW.id,
        'update',
        jsonb_build_object('name', OLD.name, 'price', OLD.price, 'details', OLD.details),
        jsonb_build_object('name', NEW.name, 'price', NEW.price, 'details', NEW.details));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

<br>

```sql
CREATE TRIGGER trigger_products_update_log
    AFTER UPDATE ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_update();
```

<br>

```sql
CREATE OR REPLACE FUNCTION log_products_delete()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO products_log (product_id, action, old_data)
    VALUES (
        OLD.id,
        'delete',
        jsonb_build_object('name', OLD.name, 'price', OLD.price, 'details', OLD.details));
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;
```

<br>

```sql
CREATE TRIGGER trigger_products_delete_log
    BEFORE DELETE ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_delete();
```

<br>

### Создание индексов

```sql
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_active ON products(is_active);
CREATE INDEX idx_products_price ON products(price);
CREATE INDEX idx_products_details ON products USING GIN (details);  -- для быстрого поиска внутри JSONB
CREATE INDEX idx_products_name ON products(name);
```

<br>

### Тестовые данные

#### Таблица категорий товаров

```sql
INSERT INTO categories (name, sort_order) VALUES
('Жевательный табак', 1),
('Жидкости', 2),
('Жидкости без никотина', 3),
('Ароматизаторы', 4),
('Одноразовые устройства', 5),
('Табак для кальяна', 6),
('Расходные материалы', 7),
('ЭСДН', 8),
('Сигариллы и альтернатива', 9),
('Напитки и снэки', 10),
('Баки, моды и аксессуары', 11),
('Ликвидация', 12);
```

<br>

===================================

***!!!*** ***Информация взята со [страницы каталога]([https://vape-shop43.ru/catalog/zhevatelnyi-tabak/adex-1](https://vape-shop43.ru/catalog))*** ***!!!***

===================================

<br>

#### Таблица товара

```sql
INSERT INTO products (category_id, name, price, details) VALUES (
    1,
    'ADEX',
    520.00,
    '{
    "strength": ["Strong", "Medium"],
    "size": ["Wide", "Slim", "Mini"],
    "flavors": ["Ice Mint", "Cold Dry", "Ice Cool", "Eucaliptus"]
    }'::JSONB);
```

<br>

==================================

***!!!*** ***Информация взята со [страницы товара](https://vape-shop43.ru/catalog/zhevatelnyi-tabak/adex-1)*** ***!!!***

==================================

<br>

## Определение необходимых таблиц (другое)

### Таблица пользователей

```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    tg_id BIGINT NOT NULL UNIQUE,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    username VARCHAR(100),
    birthday DATE,
    city_id INTEGER REFERENCES cities(id) ON DELETE SET NULL,
    is_admin BOOLEAN DEFAULT FALSE,
    is_blocked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_activity TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
```

<br>

### Таблица городов

```sql
CREATE TABLE cities (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE);
```

<br>

### Таблица контактных данных

```sql
CREATE TABLE contacts (
    id SERIAL PRIMARY KEY,
    city_id INTEGER NOT NULL REFERENCES cities(id) ON DELETE CASCADE,
    email VARCHAR(255),
    social_links JSONB NOT NULL,
    phones JSONB NOT NULL', 
    addresses JSONB NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
```

### Тестовые данные

```sql
INSERT INTO cities (name) VALUES
('Сыктывкар'),
('Киров');
```

<br>

```sql
INSERT INTO contacts (city_id, email, social_links, phones, addresses) VALUES
(
    (SELECT id FROM cities WHERE name = 'Киров'),
    'magazinestche@gmail.com',
    jsonb_build_object(
        'vk', 'https://vk.com/hqd_pods_original',
        'telegram', 'https://t.me/est_che_43'
    ),
    '["+79123379797"]'::JSONB,
    '[
        {
            "address": "Герцена 87а",
            "schedule": "ежедневно 11:00–23:00"
        },
        {
            "address": "Преображенская 57",
            "schedule": "ежедневно 10:00–22:00"
        },
        {
            "address": "Воровского 137/1",
            "schedule": "ежедневно 10:00–22:00"
        },
        {
            "address": "Ленина 102а",
            "schedule": "ежедневно 10:00–22:00"
        },
        {
            "address": "Пролетарская 22а",
            "schedule": "ежедневно 10:00–00:00"
        },
        {
            "address": "Советская 39",
            "schedule": "ежедневно 10:00–22:00"
        },
        {
            "address": "Луганская 53/2",
            "schedule": "ежедневно 10:00–21:00"
        }
    ]'::JSONB
);
```

<br>

```sql
INSERT INTO contacts (city_id, email, social_links, phones, addresses) VALUES
(
    (SELECT id FROM cities WHERE name = 'Сыктывкар'),
    'magazinestche@gmail.com',
    jsonb_build_object(
        'vk', 'https://vk.com/est_che_store11',
        'telegram', 'https://t.me/est_che_11'
    ),
    '["+79539434455"]'::JSONB,
    '[
        {
            "address": "Коммунистическая 19",
            "schedule": "ежедневно 10:00–22:00"
        },
        {
            "address": "Бумажников 53 Д",
            "schedule": "ежедневно 10:00–22:00"
        }
    ]'::JSONB
);
```

### Запрос обновления данных
```sql
UPDATE contacts 
SET 
    email = 'magazinestche@gmail.com',
    social_links = jsonb_build_object(
        'vk', 'https://vk.com/hqd_pods_original',
        'telegram', 'https://t.me/est_che_43'
    ),
    phones = '["+79123379797"]'::JSONB,
    addresses = '[
        {"address": "Герцена 87а", "schedule": "ежедневно 11:00–23:00"},
        {"address": "Преображенская 57", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Воровского 137/1", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Ленина 102а", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Пролетарская 22а", "schedule": "ежедневно 10:00–00:00"},
        {"address": "Советская 39", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Луганская 53/2", "schedule": "ежедневно 10:00–21:00"}
    ]'::JSONB,
    updated_at = CURRENT_TIMESTAMP
WHERE city_id = (SELECT id FROM cities WHERE name = 'Киров');
```

<br>

```sql
UPDATE contacts 
SET 
    email = 'magazinestche@gmail.com',
    social_links = jsonb_build_object(
        'vk', 'https://vk.com/est_che_store11',
        'telegram', 'https://t.me/est_che_11'
    ),
    phones = '["+79539434455"]'::JSONB,
    addresses = '[
        {"address": "Коммунистическая 19", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Бумажников 53 Д", "schedule": "ежедневно 10:00–22:00"}
    ]'::JSONB,
    updated_at = CURRENT_TIMESTAMP
WHERE city_id = (SELECT id FROM cities WHERE name = 'Сыктывкар');
```

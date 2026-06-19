# Категории товаров

## Информация взята со [страницы каталога](https://vape-shop43.ru/catalog)

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

# Тестовый товар

## Информация взята со [страницы товара](https://vape-shop43.ru/catalog/zhevatelnyi-tabak/adex-1)

![Adex](https://vape-shop43.ru/uploads/thumbs/webp/rc/SlQl0c4j/uploads/1-/1-69736dd14f1b8380157479.webp)

```sql
INSERT INTO products (category_id, name, price, details) VALUES (
    1,
    'ADEX',
    520.00,
    '{
        "strength": ["Strong", "Medium"],
        "size": ["Wide", "Slim", "Mini"],
        "flavors": ["Ice Mint", "Cold Dry", "Ice Cool", "Eucaliptus"]
    }'::JSONB
);
```

# Города

```sql
INSERT INTO cities (name) VALUES
('Киров'),
('Сыктывкар')
ON CONFLICT (name) DO NOTHING;
```

# Контактная информация

## Информация взята со [страницы контактов](https://vape-shop43.ru/contacts)
## Контакты для Кирова

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

## Контакты для Сыктывкара

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

# Обновление контактов для Кирова

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

# Обновление контактов для Сыктывкара

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

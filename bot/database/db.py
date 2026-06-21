import asyncpg
from typing import Optional, Dict, Any, List
from datetime import datetime, date

class Database:
    def __init__(self, pool: asyncpg.Pool):
        self.pool = pool
        
    async def create_user_if_not_exists(
        self, 
        tg_id: int, 
        first_name: str = None, 
        last_name: str = None, 
        username: str = None):
        query = """
            INSERT INTO users (tg_id, first_name, last_name, username, created_at, last_activity)
            VALUES ($1, $2, $3, $4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            ON CONFLICT (tg_id) DO NOTHING"""
        async with self.pool.acquire() as conn:
            await conn.execute(query, tg_id, first_name, last_name, username)
    
    async def get_user_status(self, tg_id: int) -> Optional[Dict]:
        query = """
            SELECT 
                age_verified,
                agreed_to_terms,
                blocked_until,
                verification_attempts,
                birth_date
            FROM users
            WHERE tg_id = $1"""
        async with self.pool.acquire() as conn:
            row = await conn.fetchrow(query, tg_id)
            if row:
                return dict(row)
            return None
    
    async def check_user_access(self, tg_id: int) -> tuple:
        query = """
            SELECT can_access, reason, age_verified, agreed_to_terms, 
                   is_blocked, attempts_left, block_until, age
            FROM check_user_access($1)"""
        async with self.pool.acquire() as conn:
            row = await conn.fetchrow(query, tg_id)
            if row:
                return (
                    row['can_access'],
                    row['reason'],
                    row['age_verified'],
                    row['agreed_to_terms'],
                    row['is_blocked'],
                    row['attempts_left'],
                    row['block_until'],
                    row['age'])
            return (False, "Пользователь не найден", False, False, False, 0, None, None)
    
    async def set_birth_date(self, tg_id: int, birth_date: date) -> tuple:
        query = "SELECT * FROM set_user_birth_date($1, $2)"
        async with self.pool.acquire() as conn:
            row = await conn.fetchrow(query, tg_id, birth_date)
            if row:
                return row['success'], row['message'], row['is_adult']
            return False, "Ошибка", False
    
    async def accept_terms(self, tg_id: int) -> tuple:
        query = "SELECT * FROM accept_terms($1)"
        async with self.pool.acquire() as conn:
            row = await conn.fetchrow(query, tg_id)
            if row:
                return row['success'], row['message']
            return False, "Ошибка"
    
    async def decline_terms(self, tg_id: int) -> tuple:
        query = "SELECT * FROM decline_terms($1)"
        async with self.pool.acquire() as conn:
            row = await conn.fetchrow(query, tg_id)
            if row:
                return row['success'], row['message'], row['attempts_left'], row['is_blocked']
            return False, "Ошибка", 0, False
    
    async def reset_verification(self, tg_id: int) -> bool:
        query = "SELECT reset_verification($1)"
        async with self.pool.acquire() as conn:
            return await conn.fetchval(query, tg_id)
    
    async def is_admin(self, tg_id: int) -> bool:
        query = """
            SELECT EXISTS (
                SELECT 1 FROM admins 
                WHERE login = $1::TEXT AND is_active = true AND is_locked = false)"""
        async with self.pool.acquire() as conn:
            return await conn.fetchval(query, str(tg_id))
    
    async def get_categories(self) -> List[Dict]:
        query = """
            SELECT id, name, sort_order 
            FROM categories 
            ORDER BY sort_order, name"""
        async with self.pool.acquire() as conn:
            rows = await conn.fetch(query)
            return [dict(row) for row in rows]
    
    async def get_products_by_category(self, category_id: int, limit: int = 50) -> List[Dict]:
        query = """
            SELECT 
                p.id,
                p.name,
                p.price,
                p.image_url,
                p.description,
                p.details,
                b.name as brand_name,
                c.name as category_name
            FROM products p
            LEFT JOIN brands b ON b.id = p.brand_id
            JOIN categories c ON c.id = p.category_id
            WHERE p.category_id = $1 AND p.is_active = true
            ORDER BY p.name
            LIMIT $2"""
        async with self.pool.acquire() as conn:
            rows = await conn.fetch(query, category_id, limit)
            return [dict(row) for row in rows]
    
    async def get_product(self, product_id: int) -> Optional[Dict]:
        query = """
            SELECT 
                p.id,
                p.name,
                p.price,
                p.image_url,
                p.description,
                p.details,
                b.name as brand_name,
                c.name as category_name
            FROM products p
            LEFT JOIN brands b ON b.id = p.brand_id
            JOIN categories c ON c.id = p.category_id
            WHERE p.id = $1 AND p.is_active = true"""
        async with self.pool.acquire() as conn:
            row = await conn.fetchrow(query, product_id)
            return dict(row) if row else None
    
    async def search_products(self, query: str, category_id: int = None, brand_id: int = None) -> List[Dict]:
        sql = """
            SELECT 
                p.id,
                p.name,
                p.price,
                p.image_url,
                p.description,
                b.name as brand_name,
                c.name as category_name
            FROM products p
            LEFT JOIN brands b ON b.id = p.brand_id
            JOIN categories c ON c.id = p.category_id
            WHERE p.is_active = true"""
        params = []
        param_count = 1
        if query:
            sql += f" AND (p.name ILIKE ${param_count} OR p.description ILIKE ${param_count})"
            params.append(f"%{query}%")
            param_count += 1
        if category_id:
            sql += f" AND p.category_id = ${param_count}"
            params.append(category_id)
            param_count += 1
        if brand_id:
            sql += f" AND p.brand_id = ${param_count}"
            params.append(brand_id)
        sql += " ORDER BY p.name LIMIT 50"
        async with self.pool.acquire() as conn:
            rows = await conn.fetch(sql, *params)
            return [dict(row) for row in rows]
    
    async def get_brands_by_category(self, category_id: int) -> List[Dict]:
        query = """
            SELECT id, name 
            FROM brands 
            WHERE category_id = $1
            ORDER BY name"""
        async with self.pool.acquire() as conn:
            rows = await conn.fetch(query, category_id)
            return [dict(row) for row in rows]


    async def get_all_contacts(self) -> List[Dict]:
        query = """
            SELECT 
                c.id,
                c.email,
                c.social_links,
                c.phones,
                c.addresses,
                ci.name as city_name
            FROM contacts c
            JOIN cities ci ON ci.id = c.city_id
            ORDER BY ci.name"""
        async with self.pool.acquire() as conn:
            rows = await conn.fetch(query)
            return [dict(row) for row in rows]

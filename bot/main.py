import asyncio
import asyncpg
import logging
from aiogram import Bot, Dispatcher
from aiogram.client.default import DefaultBotProperties
from aiogram.enums import ParseMode

from config import config
from database.db import Database
from handlers import (start_router, catalog_router, product_router, search_router, contacts_router)
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

async def main():
    try:
        pool = await asyncpg.create_pool(
            host=config.DB_HOST,
            database=config.DB_NAME,
            user=config.DB_USER,
            password=config.DB_PASSWORD,
            min_size=5,
            max_size=20)
        logger.info("Подключение к БД установлено")
    except Exception as e:
        logger.error(f"Ошибка подключения к БД: {e}")
        return
    db = Database(pool)
    bot = Bot(
        token=config.BOT_TOKEN,
        default=DefaultBotProperties(parse_mode=ParseMode.HTML))
    dp = Dispatcher()
    dp['db'] = db
    dp.include_router(start_router)
    dp.include_router(catalog_router)
    dp.include_router(product_router)
    dp.include_router(search_router)
    dp.include_router(contacts_router)
    try:
        await bot.delete_webhook(drop_pending_updates=True)
        logger.info("Бот запущен")
        await dp.start_polling(bot)
    except Exception as e:
        logger.error(f"Ошибка запуска бота: {e}")
    finally:
        await pool.close()

if __name__ == "__main__":
    asyncio.run(main())

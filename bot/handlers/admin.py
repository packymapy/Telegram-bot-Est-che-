from aiogram import Router, types, F
from aiogram.filters import Command
from database.db import Database
from keyboards.inline import get_admin_keyboard
from utils.helpers import is_admin_user
from config import config
router = Router()

@router.message(Command("admin"))
async def cmd_admin(message: types.Message, db: Database):
    user_id = message.from_user.id
    is_admin = await db.is_admin(user_id) or is_admin_user(user_id, config.ADMIN_IDS)
    if not is_admin:
        await message.answer("❌ У вас нет доступа к админ-панели")
        return
    await message.answer(
        "**Админ-панель**\n\n"
        "Выберите действие:",
        parse_mode="Markdown",
        reply_markup=get_admin_keyboard())

@router.callback_query(F.data.startswith("admin_"))
async def admin_actions(callback: types.CallbackQuery, db: Database):
    action = callback.data.split("_")[1]
    if action == "products":
        await callback.message.edit_text(
            "📦 **Управление товарами**\n\n"
            "Здесь будет управление товарами",
            parse_mode="Markdown")
    
    elif action == "stats":
        total_users = 0
        total_products = 0
        await callback.message.edit_text(
            f"**Статистика**\n\n"
            f"Всего пользователей: {total_users}\n"
            f"Всего товаров: {total_products}",
            parse_mode="Markdown")
    elif action == "users":
        await callback.message.edit_text(
            "**Управление пользователями**\n\n"
            "Здесь будет управление пользователями",
            parse_mode="Markdown")
    await callback.answer()

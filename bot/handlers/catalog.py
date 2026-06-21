from aiogram import Router, types, F
from database.db import Database
from keyboards.inline import (get_category_keyboard, get_product_keyboard, get_main_menu_keyboard)
from utils.helpers import format_product_card
router = Router()

@router.callback_query(F.data == "main_catalog")
async def show_catalog(callback: types.CallbackQuery, db: Database):
    user_id = callback.from_user.id
    can_access, reason, _, _, _, _, _, _ = await db.check_user_access(user_id)
    if not can_access:
        await callback.message.edit_text(
            f"❌ {reason}\n\nНапишите /start для проверки.",
            reply_markup=get_main_menu_keyboard())
        await callback.answer()
        return
    categories = await db.get_categories()
    if not categories:
        await callback.message.edit_text(
            "📂 Каталог пуст",
            reply_markup=get_main_menu_keyboard())
        await callback.answer()
        return
    await callback.message.edit_text(
        "📂 **Каталог товаров**\n\nВыберите категорию:",
        parse_mode="Markdown",
        reply_markup=get_category_keyboard(categories))
    await callback.answer()

@router.callback_query(F.data.startswith("cat_"))
async def show_category(callback: types.CallbackQuery, db: Database):
    category_id = int(callback.data.split("_")[1])
    products = await db.get_products_by_category(category_id)
    categories = await db.get_categories()
    if not products:
        await callback.message.edit_text(
            "📭 В этой категории пока нет товаров",
            reply_markup=get_category_keyboard(categories))
        await callback.answer()
        return
    for product in products:
        text = format_product_card(product)
        if product.get('image_url'):
            await callback.message.answer_photo(
                photo=product['image_url'],
                caption=text,
                parse_mode="HTML",
                has_spoiler=True,
                reply_markup=get_product_keyboard())
        else:
            await callback.message.answer(
                text,
                parse_mode="HTML",
                reply_markup=get_product_keyboard())
    await callback.message.answer(
        "🔙 Вернуться в каталог",
        reply_markup=get_category_keyboard(categories))
    await callback.answer()

@router.callback_query(F.data == "back_to_catalog")
async def back_to_catalog(callback: types.CallbackQuery, db: Database):
    categories = await db.get_categories()
    await callback.message.delete()
    await callback.message.answer(
        "📂 **Каталог товаров**\n\nВыберите категорию:",
        parse_mode="Markdown",
        reply_markup=get_category_keyboard(categories))
    await callback.answer()

@router.callback_query(F.data == "main_menu")
async def back_to_main_menu(callback: types.CallbackQuery):
    await callback.message.edit_text(
        "🏠 **Главное меню**\n\nВыберите действие:",
        parse_mode="Markdown",
        reply_markup=get_main_menu_keyboard())
    await callback.answer()

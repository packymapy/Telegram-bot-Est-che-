from aiogram import Router, types, F
from aiogram.filters import Command
from database.db import Database
from keyboards.inline import get_category_keyboard, get_product_keyboard
from utils.helpers import format_product_card
router = Router()

@router.message(Command("catalog"))
async def show_catalog(message: types.Message, db: Database):
    user_id = message.from_user.id
    can_access, reason, _, _, _, _, _, _ = await db.check_user_access(user_id)
    if not can_access:
        await message.answer(f"❌ {reason}\n\nНапишите /start для проверки.")
        return
    categories = await db.get_categories()
    if not categories:
        await message.answer("📂 Каталог пуст")
        return
    
    await message.answer(
        "📂 **Каталог товаров**\n\nВыберите категорию:",
        parse_mode="Markdown",
        reply_markup=get_category_keyboard(categories))

@router.callback_query(F.data.startswith("cat_"))
async def show_category(callback: types.CallbackQuery, db: Database):
    category_id = int(callback.data.split("_")[1])
    products = await db.get_products_by_category(category_id)
    if not products:
        categories = await db.get_categories()
        await callback.message.edit_text(
            "📭 В этой категории пока нет товаров(",
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
    categories = await db.get_categories()
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

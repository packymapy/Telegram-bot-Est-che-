from aiogram import Router, types, F
from aiogram.filters import Command
from aiogram.fsm.context import FSMContext
from aiogram.fsm.state import State, StatesGroup
from database.db import Database
from keyboards.inline import get_main_menu_keyboard, get_search_keyboard
from utils.helpers import format_product_card
router = Router()

class SearchState(StatesGroup):
    waiting_search = State()

@router.callback_query(F.data == "main_search")
async def cmd_search(callback: types.CallbackQuery, state: FSMContext):
    await callback.message.edit_text(
        "🔍 **Поиск товаров**\n\n"
        "Введите название товара или бренд:",
        parse_mode="Markdown",
        reply_markup=get_search_keyboard())
    await state.set_state(SearchState.waiting_search)
    await callback.answer()

@router.message(SearchState.waiting_search)
async def process_search(message: types.Message, state: FSMContext, db: Database):
    query = message.text.strip()
    if len(query) < 2:
        await message.answer(
            "❌ Минимум 2 символа для поиска",
            reply_markup=get_search_keyboard())
        return
    products = await db.search_products(query)
    if not products:
        await message.answer(
            f"❌ По запросу '{query}' ничего не найдено\n\n"
            "Попробуйте изменить запрос",
            reply_markup=get_main_menu_keyboard())
        await state.clear()
        return
    await message.answer(f"🔍 Найдено {len(products)} товаров:")
    for product in products[:10]:
        text = format_product_card(product, short=True)
        if product.get('image_url'):
            await message.answer_photo(
                photo=product['image_url'],
                caption=text,
                parse_mode="HTML",
                has_spoiler=True)
        else:
            await message.answer(text, parse_mode="HTML")
    if len(products) > 10:
        await message.answer(f"... и еще {len(products) - 10} товаров")
    await message.answer(
        "🏠 Главное меню",
        reply_markup=get_main_menu_keyboard())
    await state.clear()

@router.callback_query(F.data == "cancel_search")
async def cancel_search(callback: types.CallbackQuery, state: FSMContext):
    await state.clear()
    await callback.message.edit_text(
        "🏠 **Главное меню**\n\nВыберите действие:",
        parse_mode="Markdown",
        reply_markup=get_main_menu_keyboard())
    await callback.answer()

from aiogram import Router, types, F
from aiogram.filters import Command
from aiogram.fsm.context import FSMContext
from aiogram.fsm.state import State, StatesGroup
from database.db import Database
from utils.helpers import format_product_card
router = Router()

class SearchState(StatesGroup):
    waiting_search = State()

@router.message(Command("search"))
async def cmd_search(message: types.Message, state: FSMContext):
    await message.answer(
        "🔍 **Поиск товаров**\n\n"
        "Введите название товара или бренд:",
        parse_mode="Markdown")
    await state.set_state(SearchState.waiting_search)

@router.message(SearchState.waiting_search)
async def process_search(message: types.Message, state: FSMContext, db: Database):
    query = message.text.strip()
    if len(query) < 2:
        await message.answer("❌ Минимум 2 символа для поиска")
        return
    products = await db.search_products(query)
    if not products:
        await message.answer(
            f"❌ По запросу '{query}' ничего не найдено\n\n"
            "Попробуйте изменить запрос")
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
    await state.clear()

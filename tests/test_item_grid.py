import json
from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
ITEM_FILES = [
    ROOT / "data" / "items_vanilla.json",
    ROOT / "data" / "items_mod.json",
]
MAX_PAGE = 5
MAX_ROW = 7
MAX_COLUMN = 17


class ItemGridTest(unittest.TestCase):
    def test_non_zero_item_grid_indices_are_unique_and_in_picker_bounds(self):
        seen = {}
        for path in ITEM_FILES:
            with path.open(encoding="utf-8-sig") as handle:
                for item in json.load(handle):
                    grid_index = item["GridIndex"]
                    if grid_index == 0:
                        continue

                    item_id = item["ID"]
                    page = grid_index // 1000
                    row = (grid_index - page * 1000) // 100
                    column = grid_index % 100

                    self.assertGreaterEqual(page, 1, f"{item_id} has invalid page in {grid_index}")
                    self.assertLessEqual(page, MAX_PAGE, f"{item_id} has invalid page in {grid_index}")
                    self.assertGreaterEqual(row, 1, f"{item_id} has invalid row in {grid_index}")
                    self.assertLessEqual(row, MAX_ROW, f"{item_id} has invalid row in {grid_index}")
                    self.assertGreaterEqual(column, 1, f"{item_id} has invalid column in {grid_index}")
                    self.assertLessEqual(column, MAX_COLUMN, f"{item_id} has invalid column in {grid_index}")

                    self.assertNotIn(
                        grid_index,
                        seen,
                        f"{item_id} and {seen.get(grid_index)} share item GridIndex {grid_index}",
                    )
                    seen[grid_index] = item_id


if __name__ == "__main__":
    unittest.main()

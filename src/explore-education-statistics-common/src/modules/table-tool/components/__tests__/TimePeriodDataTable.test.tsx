import {SortableOptionWithGroup} from "@common/modules/table-tool/components/TableHeadersForm";
import {createRowGroups, createIgnoreRowGroups} from "@common/modules/table-tool/components/TimePeriodDataTable";

describe('MultiHeaderTable', () => {
  test("createRowGroups", () => {

    const options: SortableOptionWithGroup[][] = [
      [
        {label: "1", value: "1", filterGroup: "default"},
        {label: "2", value: "2", filterGroup: "default"},
        {label: "3", value: "3", filterGroup: "default"},
        {label: "4", value: "4", filterGroup: "default"},
      ],
      [
        {label: "A", value: "A", filterGroup: "total"},
        {label: "Q", value: "Q", filterGroup: "Q"},
        {label: "W", value: "W", filterGroup: "Q"},
        {label: "Z", value: "Z", filterGroup: "Z"},
        {label: "X", value: "X", filterGroup: "Z"},
      ]

    ];

    const rows = createRowGroups(options);

    console.log(rows);

    const rowIgnore = createIgnoreRowGroups(options);

    console.log(rowIgnore);

  });
});
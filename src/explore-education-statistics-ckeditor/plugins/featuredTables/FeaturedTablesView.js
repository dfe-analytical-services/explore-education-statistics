/* eslint-disable no-restricted-syntax */
/* eslint-disable no-undef */
/* eslint-disable no-underscore-dangle */
import {
  View,
  LabeledFieldView,
  createLabeledInputText,
  ButtonView,
  submitHandler,
  createDropdown,
  Model,
  addListToDropdown,
} from '@ckeditor/ckeditor5-ui';
import { icons } from '@ckeditor/ckeditor5-core';
import { Collection } from '@ckeditor/ckeditor5-utils';

export default class FormView extends View {
  constructor(locale, config, commands) {
    super(locale, config, commands);
    this.selectedTable = undefined;

    const { tables } = config.get('featuredTables');

    // create the list of dropdown options
    const items = new Collection();

    tables.forEach(option => {
      items.add({
        type: 'button',
        model: new Model({
          id: option.id,
          withText: true,
          label: option.label,
        }),
      });
    });
    const accessibleLabel = 'Featured Tables';

    const dropdownView = createDropdown(locale);
    addListToDropdown(dropdownView, items, {
      ariaLabel: accessibleLabel,
      role: 'menu',
    });

    dropdownView.buttonView.set({
      ariaLabel: accessibleLabel,
      ariaLabelledBy: undefined,
      isOn: false,
      withText: true,
      tooltip: accessibleLabel,
      label: 'Select a featured table',
    });

    dropdownView.extendTemplate({
      attributes: {
        class: ['ck-heading-dropdown'],
      },
    });

    this.listenTo(dropdownView, 'execute', evt => {
      const { id, label } = evt.source;
      this.selectedTable = { id, label };

      dropdownView.buttonView.set({
        label: this.selectedTable.label,
      });
    });

    this.saveButtonView = this._createButton(
      'Save',
      icons.check,
      'ck-button-save',
    );
    // Submit type of the button will trigger the submit event on entire form when clicked
    // (see submitHandler() in render() below).
    this.saveButtonView.type = 'submit';

    this.cancelButtonView = this._createButton(
      'Cancel',
      icons.cancel,
      'ck-button-cancel',
    );

    // Delegate ButtonView#execute to FormView#cancel
    this.cancelButtonView.delegate('execute').to(this, 'cancel');

    this.childViews = this.createCollection([
      // this.labeledDropdown,
      dropdownView,
      this.saveButtonView,
      this.cancelButtonView,
    ]);

    this.setTemplate({
      tag: 'form',
      attributes: {
        class: ['ck', 'ck-featured-tables-form'],
        tabindex: '-1',
      },
      children: this.childViews,
    });
  }

  render() {
    super.render();

    // Submit the form when the user clicked the save button or pressed enter in the input.
    submitHandler({
      view: this,
    });
  }

  focus() {
    this.childViews.first.focus();
  }

  _createInput(label) {
    const labeledInput = new LabeledFieldView(
      this.locale,
      createLabeledInputText,
    );

    labeledInput.label = label;

    return labeledInput;
  }

  _createButton(label, icon, className) {
    const button = new ButtonView();

    button.set({
      label,
      icon,
      tooltip: true,
      class: className,
    });

    return button;
  }
}

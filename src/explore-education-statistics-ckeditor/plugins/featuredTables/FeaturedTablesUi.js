/* eslint-disable no-underscore-dangle */

import Plugin from '@ckeditor/ckeditor5-core/src/plugin';
import ButtonView from '@ckeditor/ckeditor5-ui/src/button/buttonview';
import { ContextualBalloon, clickOutsideHandler } from '@ckeditor/ckeditor5-ui';
import FormView from './FeaturedTablesView';
import './styles.css';

export default class FeaturedTablesUI extends Plugin {
  static get requires() {
    return [ContextualBalloon];
  }

  init() {
    const { editor } = this;

    console.log('FeaturedTablesUI', editor.config.get('featuredTables'));

    // Create the balloon and the form view.
    this._balloon = this.editor.plugins.get(ContextualBalloon);
    this.formView = this._createFormView();

    editor.ui.componentFactory.add('featuredTables', () => {
      const button = new ButtonView();

      // add icon
      button.label = 'Featured Tables';
      button.tooltip = true;
      button.withText = true;

      // Show the UI on button click.
      this.listenTo(button, 'execute', () => {
        this._showUI();
      });

      return button;
    });
  }

  _createFormView() {
    const { editor } = this;
    const formView = new FormView(
      editor.locale,
      editor.config,
      editor.commands,
    );

    // Execute the command after clicking the "Save" button.
    this.listenTo(formView, 'submit', () => {
      // get the selected table. probably a better way to do this?
      const { selectedTable } = formView;
      console.log('selectedTable', selectedTable);

      const fastTrackUrl = 'http://localhost:3000/data-tables/fast-track/'; // to do pass in as config

      editor.model.change(writer => {
        const insertPosition = editor.model.document.selection.getFirstPosition();

        writer.insertText(
          selectedTable.label,
          {
            linkHref: `${fastTrackUrl}${selectedTable.id}`,
          },
          insertPosition,
        );
      });

      // Hide the form view after submit.
      this._hideUI();
    });

    // Hide the form view after clicking the "Cancel" button.
    this.listenTo(formView, 'cancel', () => {
      this._hideUI();
    });

    // Hide the form view when clicking outside the balloon.
    clickOutsideHandler({
      emitter: formView,
      activator: () => this._balloon.visibleView === formView,
      contextElements: [this._balloon.view.element],
      callback: () => this._hideUI(),
    });

    return formView;
  }

  _showUI() {
    this._balloon.add({
      view: this.formView,
      position: this._getBalloonPositionData(),
    });

    this.formView.focus();
  }

  _hideUI() {
    // Clear the input field values and reset the form.
    this.formView.element.reset();

    this._balloon.remove(this.formView);

    // Focus the editing view after inserting the abbreviation so the user can start typing the content
    // right away and keep the editor focused.
    this.editor.editing.view.focus();
  }

  _getBalloonPositionData() {
    const { view } = this.editor.editing;
    const viewDocument = view.document;
    let target = null;

    // Set a target position by converting view selection range to DOM
    target = () =>
      view.domConverter.viewRangeToDom(viewDocument.selection.getFirstRange());

    return {
      target,
    };
  }
}

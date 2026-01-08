import { ButtonView, Plugin } from 'ckeditor5';

export default class GlossaryUI extends Plugin {
  init() {
    const { editor } = this;
    const config = editor.config.get('glossary') || {};

    editor.ui.componentFactory.add('glossary', locale => {
      const command = editor.commands.get('addGlossaryItem');
      const buttonView = new ButtonView(locale);

      buttonView.set({
        label: editor.t('Insert glossary link'),
        withText: true,
      });

      // Bind the state of the button to the command.
      buttonView.bind('isOn', 'isEnabled').to(command, 'value', 'isEnabled');

      // Execute the command when the button is clicked (executed).
      this.listenTo(buttonView, 'execute', () => {
        config.addGlossaryItem();
      });

      return buttonView;
    });
  }
}

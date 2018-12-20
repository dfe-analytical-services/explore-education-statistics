import React, { Component } from 'react';
import PropTypes from 'prop-types'
import axios from 'axios';

class Themes extends Component {

    constructor(props) {
        super(props)
        this.state = {
            store: []
        }
    }

    componentDidMount() {
        axios.get('http://localhost:5010/api/Theme')
            .then(json => this.setState({ store: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const data = this.state.store;
        const dataItems = data.map((item, index) =>
            <Theme key={item.id} label={item.title}></Theme>
        );
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <h1 className="govuk-heading-xl">Themes</h1>
                    {dataItems}
                </div>
            </div>
        );
    }
}

class Theme extends Component {
    render() {
        return (
            <h2>{this.props.label}</h2>
        );
    }
}

Theme.propTypes = {
    label: PropTypes.string
}

Theme.defaultProps = {
    label: ''
}

export default Themes;
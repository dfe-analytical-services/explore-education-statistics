import React, { Component } from 'react';
import PropTypes from 'prop-types'
import axios from 'axios';

class Topics extends Component {

    constructor(props) {
        super(props)
        this.state = {
            users: [],
            store: []
        }
    }

    componentDidMount() {
        axios.get('http://localhost:5010/api/Topic')
            .then(json => this.setState({ store: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const data = this.state.store;
        const dataItems = data.map((item, index) =>
            <Topic key={item.id} label={item.title}></Topic>
        );
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <h1 className="govuk-heading-xl">Topics</h1>
                    {dataItems}
                </div>
            </div>
        );
    }
}

class Topic extends Component {
    render() {
        return (
            <h2>{this.props.label}</h2>
        );
    }
}

Topic.propTypes = {
    label: PropTypes.string
}

Topic.defaultProps = {
    label: ''
}

export default Topics;
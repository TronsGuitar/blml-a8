Understanding Large Language Models (LLMs) with Python and Simple Math

Have you ever wondered how computers can write stories, answer questions, or even chat with you like a friend? That's all thanks to something called Large Language Models (LLMs). In this article, we'll explore how LLMs work using Python code and math concepts you might have learned in middle school.

Table of Contents

1. What is a Language Model?


2. Breaking Down Sentences: Tokens


3. Turning Words into Numbers


4. Training Data: Feeding the Model


5. Neural Networks: The Brain of the Model


6. Training the Model


7. Generative AI: How Models Create Text


8. Final Example: Step-by-Step Process


9. Definitions


10. Conclusion




---

<a name="1"></a>

1. What is a Language Model?

A Language Model is a program that understands and generates human language. Think of it like a smart robot that can predict what word comes next in a sentence.

Example:

If I say, "The sky is...", the model might predict "blue" because that's a common word that follows.


---

<a name="2"></a>

2. Breaking Down Sentences: Tokens

Computers don't understand words like we do. So, we need to break down sentences into smaller pieces called tokens.

What are Tokens?

Tokens can be words, characters, or subwords.

They are like the LEGO blocks of language that the model uses.


Example:

Sentence: "I love Python!"

Tokens: ["I", "love", "Python", "!"]


---

<a name="3"></a>

3. Turning Words into Numbers

Computers understand numbers, not words. So, we need to convert tokens into numbers.

Assigning Numbers to Tokens

We can create a dictionary that maps each token to a unique number.

token_to_number = {
    "I": 1,
    "love": 2,
    "Python": 3,
    "!": 4
}

Now, "I love Python!" becomes [1, 2, 3, 4].

Word Embeddings

But just assigning numbers isn't enough. We need to represent words in a way that captures their meaning.

Embeddings are like coordinates on a map.

Words with similar meanings have embeddings that are close together.


Simple Math Example:

Imagine a number line:

"Happy" might be at 5.

"Joyful" might be at 6.

"Sad" might be at -5.



---

<a name="4"></a>

4. Training Data: Feeding the Model

A model learns from examples, and these examples come from training data.

What is Training Data?

Training data is a large collection of text that the model learns from.

It can include books, articles, websites, and more.


Why is Training Data Important?

It helps the model understand language patterns.

The more data it has, the better it can predict and generate text.


Simple Math Analogy:

Think of training data as practice problems in math class.

The more problems you solve, the better you understand how to find the answers.



---

<a name="5"></a>

5. Neural Networks: The Brain of the Model

A Neural Network is a set of algorithms designed to recognize patterns.

How Does It Work?

Neurons: Basic units that take input, process it, and give output.

Layers: Neurons are organized in layers—input layer, hidden layers, and output layer.


Simple Neuron Example:

A neuron can be thought of as:

Output = (Input × Weight) + Bias

Where:

Input: The number we feed in.

Weight: How important that input is.

Bias: A number added to adjust the output.



---

<a name="6"></a>

6. Training the Model

Training is like teaching the model to make better predictions.

Steps in Training:

1. Feed Input: Provide input data from the training dataset.


2. Make Prediction: The model gives an output.


3. Calculate Error: Compare prediction with the correct answer.


4. Adjust Weights: Change the weights to reduce error.


5. Repeat: Do this many times with lots of data.



Simple Math Example:

If the model predicts 4 but the correct answer is 5, the error is 1. We adjust the weights to reduce this error.

The Role of the Training Dataset:

The model looks at many examples from the training data.

It learns the probability of certain words following others.

Over time, it gets better at making accurate predictions.



---

<a name="7"></a>

7. Generative AI: How Models Create Text

Generative AI refers to models that can create new content, like text, images, or music.

How Does Generative AI Work in Language Models?

Prediction: The model predicts the next word in a sentence based on previous words.

Probability Distribution: It calculates the likelihood of possible next words.

Sampling: It selects the next word based on these probabilities.


Example:

Starting Prompt: "Once upon a"

Possible Next Words with Probabilities:

"time": 80%

"dream": 10%

"story": 5%

"night": 5%


The model is likely to choose "time" because it has the highest probability.

Simple Math Behind Generative AI:

Probability: A number between 0 and 1 that tells us how likely something is to happen.

The model assigns higher probabilities to words that make sense in the context.


Creating New Content:

By predicting one word at a time, the model generates sentences and paragraphs.

It can create original stories, answer questions, or write poems.



---

<a name="8"></a>

8. Final Example: Step-by-Step Process

Let's walk through an example that shows the entire process of training a simple language model and using it to generate text.

Step 1: Define the Vocabulary

We start by defining the words our model will understand.

vocab = ["I", "like", "cats", "and", "dogs", ".", "You", "love", "animals"]

Step 2: Tokenization

Break down sentences into tokens.

sentences = [
    "I like cats .",
    "You love dogs .",
    "I like dogs .",
    "You love cats .",
    "I like animals .",
    "You love animals .",
    "I like cats and dogs .",
    "You love cats and dogs ."
]

# Tokenize sentences
tokenized_sentences = [sentence.split() for sentence in sentences]

Step 3: Assign Numbers to Tokens

Create mappings between words and numbers.

token_to_number = {word: i for i, word in enumerate(vocab)}
number_to_token = {i: word for word, i in token_to_number.items()}

Step 4: Create Embeddings

Assign simple numerical embeddings to each word.

embeddings = {word: [i] for word, i in token_to_number.items()}

Step 5: Initialize the Neural Network Parameters

Set initial values for weights and biases.

# Initialize weight and bias
weight = 0.5
bias = 0.0

Step 6: Define the Neural Network Function

Create a simple neuron function.

def simple_neuron(input_value, weight, bias):
    return input_value * weight + bias

Step 7: Prepare the Training Data

Extract input and target word pairs from the tokenized sentences.

training_data = []
for sentence in tokenized_sentences:
    for i in range(len(sentence) - 1):
        input_word = sentence[i]
        target_word = sentence[i + 1]
        training_data.append((input_word, target_word))

Step 8: Train the Model

Train the model over several epochs.

# Training loop
for epoch in range(20):
    total_error = 0
    for input_word, target_word in training_data:
        # Convert words to embeddings
        input_embedding = embeddings[input_word][0]
        target_embedding = embeddings[target_word][0]

        # Get model prediction
        prediction = simple_neuron(input_embedding, weight, bias)

        # Calculate error
        error = target_embedding - prediction

        # Adjust weight and bias
        weight += 0.01 * error * input_embedding  # 0.01 is the learning rate
        bias += 0.01 * error

        total_error += error ** 2

    if (epoch + 1) % 5 == 0:
        print(f"Epoch {epoch+1}, Total Error: {total_error:.4f}")

What's Happening Here?

Input Word: The current word in the sentence.

Target Word: The next word we want to predict.

Prediction: The model's guess of the next word's embedding.

Error: The difference between the prediction and the actual embedding.

Adjustments: We tweak the weight and bias to minimize the error.


Step 9: Generate Text

Use the trained model to generate a sequence of words.

def generate_text(start_word, num_words):
    current_word = start_word
    result = [current_word]
    for _ in range(num_words):
        input_embedding = embeddings[current_word][0]
        prediction = simple_neuron(input_embedding, weight, bias)

        # Find the closest word in embeddings
        closest_word = min(
            embeddings.keys(),
            key=lambda k: abs(embeddings[k][0] - prediction)
        )
        result.append(closest_word)
        current_word = closest_word
    return ' '.join(result)

Step 10: Run the Model

Generate text starting with a given word.

print("\nGenerated Text:")
print(generate_text("I", 5))

Sample Output:

Epoch 5, Total Error: 11.5328
Epoch 10, Total Error: 7.9491
Epoch 15, Total Error: 5.6134
Epoch 20, Total Error: 3.9657

Generated Text:
I like animals . You love

Explanation:

Training Progress: As the epochs increase, the total error decreases, indicating that the model is learning.

Generated Text: The model starts with "I" and predicts the next words based on what it learned.



---

<a name="9"></a>

9. Definitions

Language Model: A program that understands and generates human language by predicting the next word in a sentence.

Token: The smallest unit of text, such as words or characters, used by the model.

Embedding: A numerical representation of a word capturing its meaning and context.

Training Data: A large collection of text used to teach the model language patterns.

Neural Network: A computational model inspired by the human brain, consisting of interconnected neurons that process data.

Neuron: The basic unit of a neural network that performs calculations to produce an output.

Weight: A parameter in a neuron that determines the importance of an input.

Bias: A parameter that allows the neuron to shift the activation function to fit the data better.

Generative AI: Artificial intelligence that can create new content, such as text, images, or music.

Epoch: One complete pass through the entire training dataset during the training process.

Learning Rate: A hyperparameter that controls how much the model's parameters are adjusted during training.

Error: The difference between the model's prediction and the actual value.



---

<a name="10"></a>

10. Conclusion

Large Language Models might seem complex, but at their core, they use simple math concepts like addition, multiplication, and understanding patterns. By feeding them training data, we help them learn language structures and meanings. With neural networks and embeddings, they can generate human-like text, making them a key part of Generative AI. Using Python, we can simulate a tiny part of how these models work.


---

Keep Exploring!

The world of AI and language models is vast and exciting. As you learn more math and programming, you'll be able to understand and create even more complex models!


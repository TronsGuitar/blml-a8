This is the fourth installment in my series on delimiters in languages. I recommend reading the first article to understand the implication of categorizing XML tags as delimiters in this context. (March 2, 2026: Updated the considerations around the training vs. model inference framework).

The tour de force of Claude is to have made XML tags first-class citizens. This assertion may seem provocative, but I believe there is something fundamental at play here.

The Claude API Docs provide practical prompting best practices, designed for developers by outlining specific instructions and clear formatting rules. They present a contrast between Claude’s modern approach and the reiterated suggestion of using traditional XML tags:



This is not a minor tip: users report that structuring prompts with XML can be a transformative experience: “Here’s the simple trick. Instead of just asking Claude stuff like normal, you put your request in special [XML] tags. . . . That’s literally it. And the results are so much better.”

And not only does Claude leverages XML tags in user prompts, but its framework specifically incorporates them as key elements: “Anthropic heavily uses XML tags in their prompts.”

This is, admittedly, a subjective reading, but I believe the repurposing of XML, a technology dating back to 1998, may represent a core aspect of what makes Claude distinctive: it turns Claude into something closer to a genuine language interpreter.

My own research (as a hobbyist) has led me to postulate the existence of a universal principle underlying all languages, whether human or artificial. I have observed this principle at work in diverse contexts: programming languages, bacterial DNA sequences, Homeric verses, and now, seemingly, with Claude. This principle centers on the necessity for any language (regardless of its form) to possess a mechanism for signaling the transition from first-order to second-order expressions. I contend that such a mechanism is fundamentally required for information transfer between any two entities; without it, meaningful communication becomes virtually impossible.

These transitions are typically indicated by markers or delimiters. In contemporary English, quotation marks serve this purpose. They delineate the shift from direct statement to reported speech, metaphor, or quoted material. These markers operate in pairs: one initiates the transition from first-order to second-order expression, while the other signals a return to the original level of discourse. Furthermore, this nesting can be deeply embedded; we can move from order n to order n+1, then to n+2, and so on, creating complex layers of meaning.

To illustrate how these distinctions play out in practice, consider an observation from an AWS prompt engineering course. It serves as a concrete demonstration of how crucial clear delimiters are for ensuring Claude accurately interprets and executes complex prompts:


“Here, Claude thinks ‘Yo Claude’ is part of the email it’s supposed to rewrite!” is a remarkably revealing statement. “Yo Claude” is a first-order expression (the user interacting with Claude), the content of the email is a second-order expression (the email the user will address to someone else). And they use XML tags because they need to delimit, they need to enclose the higher-order expression, like we do, in English, when we quote someone using quotation marks (like at the beginning of this paragraph); like Homer did when making heroes talk using formulaic delimiters in Ancient Greek; like bacterial DNA does to store recognition sequences.

In truth, it does not matter that these tags are XML. Other models use ad hoc delimiters (as explained in a previous article; example: <|begin_of_text|> and <|end_of_text|>) and Claude team could have done the same for their own prompts. What matters is what these tags represent. What makes Claude special is that its creators made it “aware” of the concept of delimiters, which, at least this is my view, is so crucial to the effective processing and communication of information. And it is precisely this capacity that makes Claude so effective at interpreting layered meaning.

# Operação Saideira: Churrasco na Laje

Jogo 2D de coleta e entrega, com visual em pixel art, ambientado em um churrasco na laje.

**Autor:** Enzo Provenza Trivellato Ristori  
**Código-fonte:** [github.com/EnzuBR/introUnity](https://github.com/EnzuBR/introUnity)

## Como jogar

Controle o personagem destacado, recolha as garrafas e leve-as ao **isopor no canto superior direito**. Basta encostar nas garrafas para coletar e se aproximar do isopor para entregar automaticamente.

Você pode carregar **até 3 garrafas por viagem**. Quando estiver com três, volte ao isopor; as garrafas que ficaram no chão continuam disponíveis. Para concluir uma fase, é necessário entregar todas as garrafas dentro do tempo.

| Fase | Garrafas para entregar | Tempo | Convidados | Poças |
| --- | ---: | ---: | ---: | ---: |
| 1 | 6 | 30 segundos | 2 | 0 |
| 2 | 9 | 35 segundos | 3 | 2 |
| 3 | 12 | 40 segundos | 4 | 4 |

Você tem **3 vidas**. Cada esbarrão em um convidado tira uma vida; após o dano, há 1,6 segundo de proteção. O tempo ou as vidas acabarem causa derrota. Depois de uma derrota, você pode repetir a mesma fase. Depois de uma vitória, pode avançar; entregar tudo na terceira fase conclui a campanha.

Os convidados dançarinos patrulham a laje. Os de camisa mostarda também patrulham, mas passam a seguir você quando está perto e à vista. As poças reduzem sua velocidade enquanto você passa por elas. Use as passagens entre as mesas, o isopor, a churrasqueira e a caixa de som para desviar. Os convidados ficam mais rápidos nas fases seguintes.

O resultado mostra o tempo gasto na fase e, ao concluir a campanha, o tempo total das três fases. Voltar ao menu e começar novamente inicia a primeira fase.

## Controles

| Ação | Teclado e mouse | Controle Xbox |
| --- | --- | --- |
| Mover | WASD ou setas | Analógico esquerdo ou direcional |
| Navegar nos menus | Mouse ou setas | Analógico ou direcional |
| Confirmar nos menus | Clique ou Enter | A |
| Pausar ou continuar | Esc | Start |
| Ligar ou desligar o som | M | — |
| Coletar e entregar | Automático por proximidade | Automático por proximidade |

## Abrir e compilar

1. Abra a pasta do projeto pelo Unity Hub com o **Unity 6000.3.26f1** e o módulo **WebGL Build Support** instalado.
2. Abra a cena `Assets/Saideira/Scenes/Laje.unity` e pressione **Play** para jogar no editor.
3. Em **File > Build Profiles**, selecione a plataforma **Web** e gere uma compilação em `Builds/WebGL`. A cena da laje já está configurada para a compilação.
4. Para publicar no itch.io, compacte o conteúdo de `Builds/WebGL`, mantendo `index.html` na raiz do ZIP, e envie como um jogo HTML.

## Créditos e referências

- Base de aprendizado: [Tutorial — Introdução Rápida da disciplina](https://slender-ceder-1da.notion.site/Tutorial-Introdu-o-R-pida-1a6d1ae1773780a7a59cf823a8b599ec).
- Referência técnica: [Documentação oficial do Unity](https://docs.unity3d.com/).
- **Tiny5:** The Tiny5 Project Authors, copyright 2022–2024. [Projeto da fonte](https://github.com/Gissio/font_tiny5). Licença SIL Open Font License 1.1 em `Assets/Saideira/Fonts/Tiny5-OFL.txt`.
- **Chakra Petch:** The Chakra Petch Project Authors, copyright 2018. [Projeto da fonte](https://github.com/m4rc1e/Chakra-Petch.git). Licença SIL Open Font License 1.1 em `Assets/Saideira/Fonts/ChakraPetch-OFL.txt`.
- Arte e áudio produzidos para este projeto, sem músicas ou samples externos.
